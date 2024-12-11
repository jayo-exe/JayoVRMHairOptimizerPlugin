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
using JayoVRMHairOptimizerPlugin.VNyanPluginHelper;

namespace JayoVRMHairOptimizerPlugin
{
    public class JayoVRMHairOptimizerPlugin : MonoBehaviour, VNyanInterface.IButtonClickedHandler
    {
        public GameObject windowPrefab;
        public GameObject window;

        private MainThreadDispatcher mainThread;
        private JayoVRMHairOptimizer hairOptimizer;

        private VNyanHelper _VNyanHelper;
        private VNyanPluginUpdater updater;

        private GameObject statusText;
        private GameObject infoText;
        private GameObject enableButton;
        private GameObject disableButton;
        private GameObject autoStartToggle;

        private bool enableOnStart;

        private string currentVersion = "v0.1.0";
        private string repoName = "jayo-exe/JayoVRMHairOptimizerPlugin";
        private string updateLink = "https://jayo-exe.itch.io/vrm-hair-optimizer-for-vnyan";

        private void OnApplicationQuit()
        {
            // Save settings
            savePluginSettings();
        }

        public void Awake()
        {

            Debug.Log($"Hair Optimizer Plugin is Awake!");
            _VNyanHelper = new VNyanHelper();

            updater = new VNyanPluginUpdater(repoName, currentVersion, updateLink);
            updater.OpenUrlRequested += (url) => mainThread.Enqueue(() => { Application.OpenURL(url); });

            enableOnStart = false;

            //Debug.Log($"Loading Settings");
            loadPluginSettings();
            updater.CheckForUpdates();

            //Debug.Log($"Beginning Plugin Setup");

            mainThread = gameObject.AddComponent<MainThreadDispatcher>();
            hairOptimizer = gameObject.AddComponent<JayoVRMHairOptimizer>();
            hairOptimizer.OptimizerStatusChanged += OnStatusChanged;
            hairOptimizer.OptimizerInfoChanged += OnInfoChanged;

            HairOptimizerTriggerHandler.EnableRequested += () => { enableOptimizer(); };
            HairOptimizerTriggerHandler.DisableRequested += () => { disableOptimizer(); };

            _VNyanHelper.registerTriggerListener(HairOptimizerTriggerHandler.GetInstance());
            
            try
            {
                window = _VNyanHelper.pluginSetup(this, "VRM Hair Optimizer", windowPrefab);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }

            // Hide the window by default
            if (window != null)
            {
                statusText = window.transform.Find("Panel/StatusText").gameObject;
                infoText = window.transform.Find("Panel/InfoText").gameObject;
                enableButton = window.transform.Find("Panel/EnableButton").gameObject;
                disableButton = window.transform.Find("Panel/DisableButton").gameObject;
                autoStartToggle = window.transform.Find("Panel/AutoStartToggle").gameObject;

                window.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                window.SetActive(false);

                try
                {
                    //Debug.Log($"Preparing Plugin Window");

                    updater.PrepareUpdateUI(
                        window.transform.Find("Panel/VersionText").gameObject,
                        window.transform.Find("Panel/UpdateText").gameObject,
                        window.transform.Find("Panel/UpdateButton").gameObject
                    );

                    window.transform.Find("Panel/TitleBar/CloseButton").GetComponent<Button>().onClick.AddListener(() => { closePluginWindow(); });
                    
                    enableButton.GetComponent<Button>().onClick.AddListener(() => { enableOptimizer(); });
                    disableButton.GetComponent<Button>().onClick.AddListener(() => { disableOptimizer(); });
                    
                    autoStartToggle.GetComponent<Toggle>().onValueChanged.AddListener((v) => { enableOnStart = v; });
                    autoStartToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(enableOnStart);

                    if(enableOnStart) enableOptimizer();
                    else disableOptimizer();
                }
                catch (Exception e)
                {
                    Debug.Log($"[HairOptimizer] Couldn't prepare Plugin Window: {e.Message}");
                }
            }
        }

        public void loadPluginSettings()
        {
            Dictionary<string, string> settings = _VNyanHelper.loadPluginSettingsData("JayoVRMHairOptimizerPlugin.cfg");
            if (settings != null)
            {
                string enableOnStartValue;
                settings.TryGetValue("EnableOnStart", out enableOnStartValue);
                if (enableOnStartValue != null) enableOnStart = Boolean.Parse(enableOnStartValue);
            }
        }

        public void OnStatusChanged(string newStatus)
        {
            mainThread.Enqueue(() => { statusText.GetComponent<Text>().text = newStatus; });
        }

        public void OnInfoChanged(string newInfo)
        {
            mainThread.Enqueue(() => { infoText.GetComponent<Text>().text = newInfo; });
        }

        public void savePluginSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["EnableOnStart"] = enableOnStart.ToString();


            _VNyanHelper.savePluginSettingsData("JayoVRMHairOptimizerPlugin.cfg", settings);
        }

        public void pluginButtonClicked()
        {
            // Flip the visibility of the window when plugin window button is clicked
            if (window != null)
            {
                window.SetActive(!window.activeSelf);
                if (window.activeSelf)
                {
                    window.transform.SetAsLastSibling();
                }
                window.transform.SetAsLastSibling();
            }
        }

        public void closePluginWindow()
        {
            window.SetActive(false);
        }

        public void enableOptimizer()
        {
            if (hairOptimizer.isRunning) return;
            hairOptimizer.Activate();
            enableButton.SetActive(false);
            disableButton.SetActive(true);
        }

        public void disableOptimizer()
        {
            if (!hairOptimizer.isRunning) return;
            hairOptimizer.Deactivate();
            enableButton.SetActive(true);
            disableButton.SetActive(false);
        }

        

    }
}
