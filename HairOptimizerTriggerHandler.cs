using System;
using System.Collections.Generic;

namespace JayoVRMHairOptimizerPlugin
{
    class HairOptimizerTriggerHandler : VNyanInterface.ITriggerHandler
    {
        // use events to communicate with the plugin
        // plugin and other modules will hook into these events to handle anything that needs to happen on thier side
        public static event Action EnableRequested;
        public static event Action DisableRequested;

        private static HairOptimizerTriggerHandler _instance;

        // the prefix used to denote triggers that this plugin should respond to
        private static string prefix = "_xjho_";

        // named triggers and thier associated hander methods
        private static Dictionary<string, Action<int, int, int, string, string, string>> actionHandlers = new Dictionary<string, Action<int, int, int, string, string, string>>
        {
            ["enable"] = handleEnableOptimizer,
            ["disable"] = handleDisableOptimizer,
        };

        public static HairOptimizerTriggerHandler GetInstance()
        {
            if(_instance == null) _instance = new HairOptimizerTriggerHandler();
            return _instance; 
        }

        // general trigger router, sends relevant incoming triggers to matching handers
        public void triggerCalled(string triggerName, int value1, int value2, int value3, string text1, string text2, string text3)
        {
            if (!triggerName.StartsWith(prefix)) return;

            string triggerAction = triggerName.Substring(prefix.Length);
            if(actionHandlers.ContainsKey(triggerAction)) actionHandlers[triggerAction](value1, value2, value3, text1, text2, text3);
        }


        // handler for the _xjho_enable trigger, fires the event to signal the plugin about the request
        public static void handleEnableOptimizer(int value1, int value2, int value3, string text1, string text2, string text3)
        {
            EnableRequested.Invoke();
        }

        // handler for the _xjho_disable trigger, fires the event to signal the plugin about the request
        public static void handleDisableOptimizer(int value1, int value2, int value3, string text1, string text2, string text3)
        {
            DisableRequested.Invoke();
        }

    }
}
