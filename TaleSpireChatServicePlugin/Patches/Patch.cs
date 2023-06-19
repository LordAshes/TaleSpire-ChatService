using System;
using System.Collections.Generic;

using BepInEx;
using GameChat.UI;
using HarmonyLib;
using UnityEngine;

namespace LordAshes
{
    public partial class ChatServicePlugin : BaseUnityPlugin
    {
        #region Patches

        [HarmonyPatch(typeof(UIChatMessageManager), "AddChatMessage")]
        public static class PatchAddChatMessage
        {
            public static bool Prefix(ref string creatureName, Texture2D icon, ref string chatMessage, UIChatMessageManager.IChatFocusable focus = null)
            {
                if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: AddEventMessage Patch"); }

                string speaker = creatureName;
                ApplyAliases(ref chatMessage);
                ProcessMessage(ref creatureName, ref chatMessage);
                if (chatMessage == null || (chatMessage.Trim() == "" && creatureName == speaker))
                {
                    return false;
                }
                if(logFileNamePrefix.Value.Trim()!="")
                {
                    LogChatMessage(creatureName, chatMessage);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UIChatMessageManager), "AddEventMessage")]
        public static class PatchAddEventMessage
        {
            public static bool Prefix(ref string title, ref string message, UIChatMessageManager.IChatFocusable focus = null)
            {
                if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: AddEventMessage Patch"); }

                string speaker = title;
                ApplyAliases(ref message);
                ProcessMessage(ref title, ref message);
                if(message==null || (message.Trim()=="" && title==speaker))
                {
                    return false;
                }
                if (logFileNamePrefix.Value.Trim() != "")
                {
                    LogEventMessage(title, message);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UIChatMessageManager), "AddDiceResultMessage")]
        public static class PatchAddDiceResultMessage
        {
            public static bool Prefix(DiceManager.RollResults diceResult, UIChatMessageManager.DiceResultsReference.ResultsOrigin origin, ClientGuid sender, bool hidden)
            {
                if (logFileNamePrefix.Value.Trim() != "")
                {
                    LogDiceResult(diceResult, sender, hidden);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ChatInputBoardTool), "Begin")]
        public static class PatchBegin
        {
            public static void Postfix()
            {
                if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: Chat Entry Begin Prefix"); }
                ChatServicePlugin.SetSpeaker();
            }
        }

        [HarmonyPatch(typeof(CreatureBoardAsset), "Speak")]
        public static class PatchSpeak
        {
            public static bool Prefix(string text)
            {
                return !CheckIfHandlersApply(text);
            }
        }

        #endregion

        #region Helpers

        private static void ApplyAliases(ref string message)
        {
            foreach(KeyValuePair<string,string> alias in aliases)
            {
                if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: ApplyAliases: Replacing '" + alias.Key+"' with '"+alias.Value+"'"); }
                message = message.Replace("/" + alias.Key, "/" + alias.Value);
            }
            if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: ApplyAliases: Alias Message = '" + message + "'"); }
        }

        private static void ProcessMessage(ref string title, ref string message)
        {
            string creatureName = title;
            if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: ParseMessage: Title = '"+Convert.ToString(title)+"' Message = '"+Convert.ToString(message)+"'"); }
            if (message.StartsWith("[") && message.Contains("]"))
            {
                title = message.Substring(1);
                title = title.Substring(0, title.IndexOf("]"));
                message = message.Substring(message.IndexOf("]") + 1).Trim();
                if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: ParseMessage: Header Adjustment: Title = '" + Convert.ToString(title) + "' Message = '" + Convert.ToString(message) + "'"); }
            }
            bool repeat;
            do
            {
                repeat = false;
                foreach (KeyValuePair<string, Func<string, string, Talespire.SourceRole, string>> handler in ChatServicePlugin.chatMessgeServiceHandlers)
                {
                    if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: ParseMessage: Found Handler '" + handler.Key + "'"); }
                    if (message.StartsWith(handler.Key))
                    {
                        if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: ParseMessage: Applying Handler '" + handler.Key + "'"); }
                        try 
                        { 
                            message = handler.Value(message, title, FindSource(creatureName)); 
                        } 
                        catch (Exception x)
                        {
                            Debug.LogWarning("Chat Service Plugin: ParseMessage: Exception In Handler: "+x.Message);
                            Debug.LogException(x);
                            message = "";
                        }
                        if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: ParseMessage: Post Handler: Title = '" + Convert.ToString(title) + "' Message = '" + Convert.ToString(message) + "'"); }
                        if (message == null) { return; }
                        if (message.Trim() == "") { return; }
                        repeat = true;
                        break;
                    }
                }
            } while (repeat);
        }

        private static bool CheckIfHandlersApply(string message)
        {
            foreach (KeyValuePair<string, Func<string, string, Talespire.SourceRole, string>> handler in ChatServicePlugin.chatMessgeServiceHandlers)
            {
                if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: CheckIfHandlersApply: Handler: '" + handler.Key+"'"); }
                if (message.StartsWith(handler.Key))
                {
                    if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: CheckIfHandlersApply: Message Uses '"+handler.Key+"' Handler"); }
                    return true;
                }
            }
            if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: CheckIfHandlersApply: Message Does Not Use Any Handler"); }
            return false;
        }

        private static Talespire.SourceRole FindSource(string name)
        {
            if (name.ToUpper() == "ANONYMOUS") { return Talespire.SourceRole.anonymous; }
            foreach(CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets)
            {
                if(asset.Name.StartsWith(name))
                {
                    return Talespire.SourceRole.creature;
                }
            }
            foreach (KeyValuePair<PlayerGuid, PlayerInfo> player in CampaignSessionManager.PlayersInfo)
            {
                if(CampaignSessionManager.GetPlayerName(player.Key)==name)
                {
                    if (player.Value.Rights.CanGm) { return Talespire.SourceRole.gm; } else { return Talespire.SourceRole.player; }
                }
            }
            return Talespire.SourceRole.other;
        }

        #endregion
    }
}
