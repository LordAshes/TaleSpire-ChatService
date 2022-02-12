using BepInEx;
using HarmonyLib;
using UnityEngine;

using System;
using System.Collections.Generic;
using Bounce.Unmanaged;

namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(FileAccessPlugin.Guid)]
    public partial class ChatServicePlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Chat Service Plug-In";
        public const string Guid = "org.lordashes.plugins.chatservice";
        public const string Version = "1.2.0.0";

        public enum ChatSource
        {
            gm = 0,
            player = 1,
            creature = 2,
            hideVolume = 3,
            other = 888,
            anonymous = 999
        }

        public static Dictionary<string, Func<string, string, ChatSource, string>> handlers = new Dictionary<string, Func<string, string, ChatSource, string>>();
        
        private static Dictionary<string, Func<string, string, Talespire.SourceRole, string>> chatMessgeServiceHandlers = new Dictionary<string, Func<string, string, Talespire.SourceRole, string>>();

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

            if (Config.Bind("Settings", "Add Deselect Option", true).Value == true)
            {
                Debug.Log("Chat Service Plugin: Creating Deselect Character Menu Option.");
                MapMenu.ItemArgs args = new MapMenu.ItemArgs()
                {
                    Title = "Deselect",
                    Icon = FileAccessPlugin.Image.LoadSprite("Deselect.png"),
                    CloseMenuOnActivate = true,
                    Action = (mmi, obj) =>
                    {
                        CreatureBoardAsset asset;
                        CreaturePresenter.TryGetAsset(LocalClient.SelectedCreatureId, out asset);
                        if (asset != null)
                        {
                            Debug.Log("Chat Service Plugin: Deselecting '" + asset.Creature.Name.Substring(0, asset.Creature.Name.IndexOf("<")) + "' (" + asset.Creature.CreatureId + ").");
                            asset.Creature.Deselect();
                            LocalClient.SelectedCreatureId = CreatureGuid.Empty;
                            ChatInputBoardTool __instance = GameObject.FindObjectOfType<ChatInputBoardTool>();
                            PatchAssistant.SetField(__instance, "_selectedCreature", null);
                        }
                    }
                };
                Debug.Log("Chat Service Plugin: Adding Deselect Character Menu Option.");
                SDIM.InvokeMethod("HolloFox_TS-RadialUIPlugin/RadialUI.dll", "AddOnCharacter", new object[] { ChatServicePlugin.Guid, args, null });
            }
        }

        void Update()
        {
        }

        public static class ChatMessageService
        {
            public static void AddHandler(string key, Func<string, string, Talespire.SourceRole, string> callback)
            {
                Debug.Log("Chat Service Plugin: Added Subscription To "+key);
                chatMessgeServiceHandlers.Add(key, callback);
            }

            public static void RemoveHandler(string key)
            {
                Debug.Log("Chat Service Plugin: Removed Subscription To " + key);
                chatMessgeServiceHandlers.Remove(key);
            }

            public static void SendMessage(string message, NGuid source)
            {
                Debug.Log("Chat Service Plugin: Sending " + message +" (signature "+source+")");
                ChatManager.SendChatMessage(message, source);
            }
        }
    }
}
