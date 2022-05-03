using System;
using System.Collections.Generic;
using System.IO;

using BepInEx;
using Bounce.Singletons;
using Bounce.Unmanaged;
using GameChat.UI;
using HarmonyLib;
using UnityEngine;

namespace LordAshes
{
    public partial class ChatServicePlugin : BaseUnityPlugin
    {
        [HarmonyPatch(typeof(UIChatMessageManager), "AddChatMessage")]
        public static class PatchAddChatMessage
        {
            public static bool Prefix(string creatureName, Texture2D icon, ref string chatMessage, UIChatMessageManager.IChatFocusable focus = null)
            {
                foreach (KeyValuePair<string, Func<string, string, Talespire.SourceRole, string>> handler in ChatServicePlugin.chatMessgeServiceHandlers)
                {
                    if (chatMessage.StartsWith(handler.Key))
                    {
                        chatMessage = handler.Value(chatMessage, creatureName, (Talespire.SourceRole)FindSource(creatureName));
                        if (chatMessage == null) { return false; }
                    }
                }
                foreach (KeyValuePair<string, Func<string, string, ChatSource, string>> handler in ChatServicePlugin.handlers)
                {
                    if (chatMessage.StartsWith(handler.Key))
                    {
                        chatMessage = handler.Value(chatMessage, creatureName, FindSource(creatureName));
                        if (chatMessage == null) { return false; }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UIChatMessageManager), "AddEventMessage")]
        public static class PatchAddEventMessage
        {
            public static bool Prefix(string title, ref string message, UIChatMessageManager.IChatFocusable focus = null)
            {
                foreach (KeyValuePair<string, Func<string, string, Talespire.SourceRole, string>> handler in ChatServicePlugin.chatMessgeServiceHandlers)
                {
                    if (message.StartsWith(handler.Key))
                    {
                        message = handler.Value(message, title, (Talespire.SourceRole)FindSource(title));
                        if (message == null) { return false; }
                    }
                }
                foreach (KeyValuePair<string, Func<string, string, ChatSource, string>> handler in ChatServicePlugin.handlers)
                {
                    if (message.StartsWith(handler.Key))
                    {
                        message = handler.Value(message, title, FindSource(title));
                        if (message == null) { return false; }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ChatInputBoardTool), "InputOnOnTextSubmit")]
        public static class PatchInputOnOnTextSubmit
        {
            public static bool Prefix(string text)
            {
                return false;
            }

            public static void Postfix(string text)
            {
                ChatInputBoardTool __instance = GameObject.FindObjectOfType<ChatInputBoardTool>();
                if (string.IsNullOrEmpty(text) || !SimpleSingletonBehaviour<ChatManager>.HasInstance)
                {
                    __instance.Back();
                }

                NGuid thingThatIsTalking = NGuid.Empty;
                if (LocalClient.SelectedCreatureId == CreatureGuid.Empty)
                {
                    thingThatIsTalking = LocalPlayer.Id.Value;
                }
                else
                {
                    thingThatIsTalking = LocalClient.SelectedCreatureId.Value;
                }

                ChatManager.SendChatMessage(text, thingThatIsTalking, null);
                __instance.Back();
            }
        }

        [HarmonyPatch(typeof(ChatInputBoardTool), "Begin")]
        public static class PatchBegin
        {
            public static bool Prefix()
            {
                return true;
            }

            public static void Postfix()
            {
                if (LocalClient.SelectedCreatureId == CreatureGuid.Empty)
                {
                    ChatInputBoardTool __instance = GameObject.FindObjectOfType<ChatInputBoardTool>();
                    UIChatInputField _input = (UIChatInputField)PatchAssistant.GetField(__instance, "_input");
                    PatchAssistant.UseMethod(_input, "UpdateSpeaker", new object[] { CampaignSessionManager.GetPlayerName(LocalPlayer.Id) });
                }
            }
        }

        private static ChatSource FindSource(string name)
        {
            if (name.ToUpper() == "ANONYMOUS") { return ChatSource.anonymous; }
            foreach(CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets)
            {
                if(asset.Name.StartsWith(name))
                {
                    return ChatSource.creature;
                }
            }
            foreach (KeyValuePair<PlayerGuid, PlayerInfo> player in CampaignSessionManager.PlayersInfo)
            {
                if(CampaignSessionManager.GetPlayerName(player.Key)==name)
                {
                    if (player.Value.Rights.CanGm) { return ChatSource.gm; } else { return ChatSource.player; }
                }
            }
            return ChatSource.other;
        }
    }
}