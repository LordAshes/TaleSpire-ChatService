using BepInEx;
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
                    Debug.Log("SDIM: Missing File");
                    return InvokeResult.missingFile; 
                }
                MethodInfo method = type.GetMethod(methodName);
                if (method == null)
                {
                    Debug.Log("SDIM: Missing Method");
                    return InvokeResult.missingMethod; 
                }
                try
                {
                    Debug.Log("SDIM: Returning Invoke Results");
                    InvokeReturn = method.Invoke(null, parameters);
                }
                catch (Exception x)
                {
                    Debug.Log("SDIM: Invalid Parameter: "+x);
                    return InvokeResult.invalidParameters;
                }
                return InvokeResult.success;
            }

            private static Type FindPlugin(string pluginFile)
            {
                Debug.Log("SDIM: Looking For " + BepInEx.Paths.PluginPath + "/" + pluginFile);
                Assembly assembly = Assembly.LoadFrom(BepInEx.Paths.PluginPath + "/" + pluginFile);
                Debug.Log("SDIM: Assemply="+Convert.ToString(assembly));
                if (assembly == null) { return null; }
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(BaseUnityPlugin))) { return type; }
                }
                return null;
            }
        }
    }
}

