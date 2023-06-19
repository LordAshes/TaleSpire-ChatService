using BepInEx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LordAshes
{
    public partial class ChatServicePlugin : BaseUnityPlugin
    {
        public static class SDIM
        {
            public enum InvokeResult
            {
                success = 0,
                missingFile = 1,
                missingMethod = 2,
                invalidParameters = 3
            }

            public static object InvokeReturn = null;

            public static InvokeResult InvokeMethod(string pluginFile, string methodName, object[] parameters)
            {
                InvokeReturn = null;

                Type type = FindPlugin(pluginFile);

                if (type == null) 
                {
                    Debug.LogWarning("Chat Service Plugin: SDIM: Missing File. Ignorning Soft Dependency Functionality.");
                    return InvokeResult.missingFile; 
                }

                MethodInfo method = type.GetMethod(methodName);
                if (method == null)
                {
                    Debug.LogWarning("Chat Service Plugin: SDIM: Missing Method. Ignorning Soft Dependency Functionality.");
                    return InvokeResult.missingMethod; 
                }
                try
                {
                    // Debug.Log("Chat Service Plugin: SDIM: Returning Invoke Results");
                    InvokeReturn = method.Invoke(null, parameters);
                    return InvokeResult.success;
                }
                catch (Exception x)
                {
                    Debug.LogWarning("Chat Service Plugin: SDIM: Failed Invoke: " + x+ ". Ignorning Soft Dependency Functionality.");
                    return InvokeResult.invalidParameters;
                }
            }

            private static Type FindPlugin(string pluginFile)
            {
                if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: SDIM: Looking For " + BepInEx.Paths.PluginPath + "/" + pluginFile); }
                if (FileAccessPlugin.File.Exists(BepInEx.Paths.PluginPath + "/" + pluginFile))
                {
                    Assembly assembly = Assembly.LoadFrom(BepInEx.Paths.PluginPath + "/" + pluginFile);
                    if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: SDIM: Assembly = " + Convert.ToString(assembly)); }
                    if (assembly == null) { return null; }
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: SDIM: Seeking 'BaseUnityPlugin' Type. Found '"+type.ToString()+"'. IsSubclassOf = "+ type.IsSubclassOf(typeof(BaseUnityPlugin))); }
                       
                        if (type.IsSubclassOf(typeof(BaseUnityPlugin))) { return type; }
                    }
                }
                else
                {
                    if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: SDIM: " + BepInEx.Paths.PluginPath + "/" + pluginFile + " Not Installed"); }
                }
                return null;
            }
        }
    }
}

