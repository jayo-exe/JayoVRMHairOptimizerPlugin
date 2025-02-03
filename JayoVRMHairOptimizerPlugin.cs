using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JayoVRMHairOptimizerPlugin.Util;
using TMPro;

namespace JayoVRMHairOptimizerPlugin
{
    public class JayoVRMHairOptimizerPlugin : MonoBehaviour, VNyanInterface.IButtonClickedHandler
    {
        public GameObject windowPrefab;
        public GameObject window;

        private JayoVRMHairOptimizer hairOptimizer;

        private static JayoVRMHairOptimizerPlugin _instance;
        private PluginUpdater updater;

        private TMP_Text statusText;
        private TMP_Text infoText;
        private Button enableButton;
        private Button disableButton;
        private Toggle autoStartToggle;

        private bool enableOnStart;

        private string currentVersion = "v0.2.0";
        private string repoName = "jayo-exe/JayoVRMHairOptimizerPlugin";
        private string updateLink = "https://jayo-exe.itch.io/vrm-hair-optimizer-for-vnyan";

        private void OnApplicationQuit()
        {
            // Save settings
            savePluginSettings();
        }

        public void Awake()
        {

            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }

            Debug.Log($"Hair Optimizer Plugin is Awake!");

            updater = new PluginUpdater(repoName, currentVersion, updateLink);
            updater.OpenUrlRequested += (url) => MainThreadDispatcher.Enqueue(() => { Application.OpenURL(url); });

            enableOnStart = false;

            //Debug.Log($"Loading Settings");
            loadPluginSettings();
            updater.CheckForUpdates();

            //Debug.Log($"Beginning Plugin Setup");

            hairOptimizer = gameObject.AddComponent<JayoVRMHairOptimizer>();
            hairOptimizer.OptimizerStatusChanged += OnStatusChanged;
            hairOptimizer.OptimizerInfoChanged += OnInfoChanged;

            HairOptimizerTriggerHandler.EnableRequested += () => { enableOptimizer(); };
            HairOptimizerTriggerHandler.DisableRequested += () => { disableOptimizer(); };

            VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(HairOptimizerTriggerHandler.GetInstance());
            
            try
            {
                VNyanInterface.VNyanInterface.VNyanUI.registerPluginButton("VRM Hair Optimizer", this);
                window = (GameObject)VNyanInterface.VNyanInterface.VNyanUI.instantiateUIPrefab(windowPrefab);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }

            // Hide the window by default
            if (window != null)
            {
                statusText = window.transform.Find("Panel/StatusRow/StatusText").GetComponent<TMP_Text>();
                infoText = window.transform.Find("Panel/StatusRow/InfoText").GetComponent<TMP_Text>();
                enableButton = window.transform.Find("Panel/ControlRow/EnableButton").GetComponent<Button>();
                disableButton = window.transform.Find("Panel/ControlRow/DisableButton").GetComponent<Button>();
                autoStartToggle = window.transform.Find("Panel/ControlRow/AutoStart/FieldHead/AutoStartToggle").GetComponent<Toggle>();

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
                    
                    enableButton.onClick.AddListener(() => { enableOptimizer(); });
                    disableButton.onClick.AddListener(() => { disableOptimizer(); });
                    
                    autoStartToggle.onValueChanged.AddListener((v) => { enableOnStart = v; });
                    autoStartToggle.SetIsOnWithoutNotify(enableOnStart);

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
            Dictionary<string, string> settings = VNyanInterface.VNyanInterface.VNyanSettings.loadSettings("JayoVRMHairOptimizerPlugin.cfg");
            if (settings != null)
            {
                string enableOnStartValue;
                settings.TryGetValue("EnableOnStart", out enableOnStartValue);
                if (enableOnStartValue != null) enableOnStart = Boolean.Parse(enableOnStartValue);
            }
        }

        public void OnStatusChanged(string newStatus)
        {
            MainThreadDispatcher.Enqueue(() => { statusText.text = newStatus; });
        }

        public void OnInfoChanged(string newInfo)
        {
            MainThreadDispatcher.Enqueue(() => { infoText.text = newInfo; });
        }

        public void savePluginSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["EnableOnStart"] = enableOnStart.ToString();

            VNyanInterface.VNyanInterface.VNyanSettings.saveSettings("JayoVRMHairOptimizerPlugin.cfg", settings);
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
            enableButton.gameObject.SetActive(false);
            disableButton.gameObject.SetActive(true);
        }

        public void disableOptimizer()
        {
            if (!hairOptimizer.isRunning) return;
            hairOptimizer.Deactivate();
            enableButton.gameObject.SetActive(true);
            disableButton.gameObject.SetActive(false);
        }

        

    }
}
