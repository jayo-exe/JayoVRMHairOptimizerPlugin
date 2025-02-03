using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq.Expressions;
using VNyanInterface;

namespace JayoVRMHairOptimizerPlugin
{

    public class JayoVRMHairOptimizer : MonoBehaviour
    {

        public event Action<string> OptimizerStatusChanged;
        public event Action<string> OptimizerInfoChanged;

        public bool isRunning {get { return running; }}
        public string currentStatus { get { return status; } }

        private GameObject lastAvatar;
        private GameObject hairObject;
        private GameObject hairsObject;
        private bool running;
        private bool optimized;
        private string status;
        private string info;
        private int beforeDrawCalls;
        private int afterDrawCalls;

        public void Awake()
        {
            running = false;
            optimized = false;
            changeStatus("Awake");
            changeInfo("");
            beforeDrawCalls = 0;
            afterDrawCalls = 0;
        }

        public void Update()
        {
            if (!running) return;

            GameObject avatar = (GameObject)VNyanInterface.VNyanInterface.VNyanAvatar.getAvatarObject();
            if (avatar == null)
            {
                changeInfo("No Avatar Found");
                return;
            }
            if (avatar == lastAvatar && optimized) return;
            lastAvatar = avatar;
            optimized = false;
            hairObject = null;
            hairsObject = null;
            //changeInfo("Avatar Changed");
            optimizeHair();
        }

        public void Activate() 
        {
            running = true;
            changeStatus("Activated");
            changeInfo("");
        }

        public void Deactivate() 
        {
            running = false;
            if(optimized) revertHair();
            changeStatus("Deactivated");
            changeInfo("");
        }

        private void changeStatus(string newStatus)
        {
            status = newStatus;
            OptimizerStatusChanged?.Invoke(newStatus);
        }

        private void changeInfo(string newInfo)
        {
            info = newInfo;
            OptimizerInfoChanged?.Invoke(newInfo);
        }

        private void optimizeHair() {
            if (optimized) return;

            GameObject avatar = (GameObject)VNyanInterface.VNyanInterface.VNyanAvatar.getAvatarObject();
            changeStatus("No avatar");
            if (!avatar) return;
            changeStatus("Begin Optimizing");
            hairsObject = avatar.transform.Find("Hairs")?.gameObject;
            if(!hairsObject)
            {
                hairObject = avatar.transform.Find("Hair")?.gameObject;
                if (hairObject)
                {
                    changeStatus("Hair already merged");
                }
                else
                {
                    changeStatus("Unknown model structure");
                }
                optimized = true;
                return;
            }


            changeStatus("Optimizing");
            doOptimize();

            changeStatus("Hair Optimized");
            changeInfo($"Before: {beforeDrawCalls} | After: {afterDrawCalls}");
            optimized = true;

        }

        private void revertHair() {
            if (!optimized) return;

            GameObject avatar = (GameObject)VNyanInterface.VNyanInterface.VNyanAvatar.getAvatarObject();
            if (!avatar)
            {
                changeStatus("No avatar to revert");
                return;
            }

            if(!hairsObject)
            {
                changeStatus("No unmerged hairs object found");
                return;
            }

            if (!hairObject)
            {
                changeStatus("No merged hairs object found");
                return;
            }

            GameObject.Destroy(hairObject);
            hairObject = null;
            hairsObject.SetActive(true);

            changeStatus("Hair Reverted");
            changeInfo("");
            optimized = false;
        }

        private Mesh FuseMeshes(Mesh smra, Mesh smrb)
        {
            if (smra == null || smrb == null)
            {
                Debug.LogError("Both meshes must be provided.");
                return smra;
            }

            Mesh combinedMesh = new Mesh();
            CombineInstance[] combine = new CombineInstance[2];

            combine[0].mesh = smra;
            combine[0].transform = Matrix4x4.identity;
            combine[1].mesh = smrb;
            combine[1].transform = Matrix4x4.identity;

            Matrix4x4[] bindposes = smra.bindposes.Length > 0 ? smra.bindposes : smrb.bindposes;

            combinedMesh.CombineMeshes(combine);

            int vertexCountA = smra.vertexCount;
            var boneWeightsA = new List<BoneWeight>();
            smra.GetBoneWeights(boneWeightsA);

            int vertexCountB = smrb.vertexCount;
            var boneWeightsB = new List<BoneWeight>();
            smrb.GetBoneWeights(boneWeightsB);

            BoneWeight[] combinedWeights = new BoneWeight[vertexCountA + vertexCountB];
            int highestSetWeight = 0;
            for (int i = 0; i < vertexCountA; i++)
            {
                combinedWeights[i] = boneWeightsA[i];
                highestSetWeight++;
            }
            for (int i = 0; i < vertexCountB; i++)
            {
                combinedWeights[vertexCountA + i] = boneWeightsB[i];
                highestSetWeight++;
            }
            combinedMesh.boneWeights = combinedWeights;
            combinedMesh.bindposes = bindposes;
            //Debug.Log("Meshes combined successfully!");
            return combinedMesh;

        }

        private Mesh BundleMeshes(Mesh[] smrSubs)
        {
            if (smrSubs == null || smrSubs.Length == 0)
            {
                Debug.LogError("Please provide an array of meshes to bundle");
                return null;
            }

            Mesh combinedMesh = new Mesh();
            CombineInstance[] combine = new CombineInstance[smrSubs.Length];
            int totalVertexCount = 0;

            for (int i = 0; i < smrSubs.Length; i++)
            {
                combine[i].mesh = smrSubs[i];
                combine[i].transform = Matrix4x4.identity;
                totalVertexCount += smrSubs[i].vertexCount;
            }
            combinedMesh.CombineMeshes(combine, false);

            BoneWeight[] combinedWeights = new BoneWeight[totalVertexCount];
            int boneIndex = 0;
            // Combine weights and bone indices
            for (int i = 0; i < smrSubs.Length; i++)
            {
                var boneWeights = new List<BoneWeight>();
                smrSubs[i].GetBoneWeights(boneWeights);

                for (int b = 0; b < smrSubs[i].vertexCount; b++)
                {
                    combinedWeights[boneIndex] = boneWeights[b];
                    boneIndex++;
                }
            }
            //Debug.Log($"Submesh Bind Poses: {smrSubs[0].bindposes.Length}");
            //Debug.Log($"Highest Bone Weight Index: {boneIndex}");
            combinedMesh.boneWeights = combinedWeights;
            combinedMesh.bindposes = smrSubs[0].bindposes;
            Debug.Log("Meshes bundled successfully!");
            return combinedMesh;
        }

        private void doOptimize()
        {
            //get all hair meshes in child of selected object
            SkinnedMeshRenderer[] hairParts = hairsObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            beforeDrawCalls = hairParts.Length;

            //create CombinedHair object as sibling of selected
            hairObject = new GameObject("Hair");
            hairObject.transform.SetParent(hairsObject.transform.parent.transform);
            hairObject.transform.localPosition = Vector3.zero;
            hairObject.transform.localRotation = Quaternion.identity;

            // prep final skinned mesh renderer
            Mesh finalMesh = new Mesh();
            SkinnedMeshRenderer finalRenderer = hairObject.AddComponent<SkinnedMeshRenderer>();
            finalRenderer.rootBone = hairParts[0].rootBone;
            finalRenderer.bones = hairParts[0].bones;
            finalRenderer.probeAnchor = hairParts[0].probeAnchor;

            //find distinct materials
            List<Material> hairMats = new List<Material>();
            for (int i = 0; i < hairParts.Length; i++)
            {
                if (!hairMats.Contains(hairParts[i].sharedMaterial)) hairMats.Add(hairParts[i].sharedMaterial);
            }
            finalRenderer.sharedMaterials = hairMats.ToArray();
            afterDrawCalls = hairMats.Count;

            //Build individual meshes by material
            Mesh[] matMeshes = new Mesh[hairMats.Count];
            Mesh lastMesh = new Mesh();
            for (int i = 0; i < hairMats.Count; i++)
            {
                Mesh accMesh = null;

                for (int b = 0; b < hairParts.Length; b++)
                {
                    if (hairParts[b].sharedMaterial == hairMats[i])
                    {
                        if (accMesh == null) accMesh = hairParts[b].sharedMesh;
                        else accMesh = FuseMeshes(accMesh, hairParts[b].sharedMesh);

                    }
                }
                accMesh.RecalculateBounds();
                matMeshes[i] = accMesh;
            }

            //combine all material objects into final hair object
            finalRenderer.sharedMesh = BundleMeshes(matMeshes);
            finalRenderer.sharedMesh.name = "hair_baked";

            hairsObject.SetActive(false);
        }
    }
}
