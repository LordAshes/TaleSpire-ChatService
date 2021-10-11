using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Collections;

namespace LordAshes
{
	[BepInPlugin(Guid, Name, Version)]
	public partial class ChatServicePlugin : BaseUnityPlugin
	{
		// Plugin info
		public const string Name = "Chat Service Plug-In";
		public const string Guid = "org.lordashes.plugins.chatservice";
		public const string Version = "1.0.1.0";

        public enum ChatSource
        {
            gm = 0,
            player = 1,
            creature = 2,
            anonymous = 999
        }

        public static Dictionary<string, Func<string, string, ChatSource, string>> handlers = new Dictionary<string, Func<string,string, ChatSource, string>>();

        /// <summary>
        /// Function for initializing plugin
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
		{
			UnityEngine.Debug.Log("Chat Service Plugin: Active.");

            UnityEngine.Debug.Log("Chat Service Plugin: Patching.");
            var harmony = new Harmony(Guid);
			harmony.PatchAll();
        }

        void Update()
        {
        }
    }
}
