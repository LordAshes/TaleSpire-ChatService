using System;
using System.Collections.Generic;
using System.IO;

using BepInEx;
using GameChat.UI;
using HarmonyLib;
using Newtonsoft.Json;
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
                foreach(KeyValuePair<string,Func<string,string, ChatSource, string>> handler in ChatServicePlugin.handlers)
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

        private static ChatSource FindSource(string name)
        {
            foreach(CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets)
            {
                if(asset.Creature.Name.StartsWith(name))
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
            return ChatSource.anonymous;
        }
    }
}