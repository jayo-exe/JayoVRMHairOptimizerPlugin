using System.IO;
using System.Reflection;
using UnityEngine;
using VNyanInterface;

namespace JayoVRMHairOptimizerPlugin;

public class JayoVRMHairOptimizerPluginManifest : IVNyanPluginManifest
{
    public string PluginName { get; } = "JayoVRMHairOptimizerPlugin";
    public string Version { get; } = "v0.2.0";
    public string Title { get; } = "Jayo's VRM Hair Optimizer Plugin";
    public string Author { get; } = "Jayo";
    public string Website { get; } = "https://jayo-exe.itch.io/vrm-hair-optimizer-for-vnyan";

    private string bundleName { get; } = "JayoVRMHairOptimizerPlugin.vnobj";

    public void InitializePlugin()
    {
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(this.GetType(), bundleName))
        {
            byte[] bundleData = new byte[stream.Length];
            stream.Read(bundleData, 0, bundleData.Length);
            AssetBundle bundle = AssetBundle.LoadFromMemory(bundleData);
            GameObject.Instantiate(bundle.LoadAsset<GameObject>(bundle.GetAllAssetNames()[0]));
        }
    }
}
