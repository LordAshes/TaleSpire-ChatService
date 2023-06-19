using BepInEx;
using HarmonyLib;
using UnityEngine;

using System;
using System.Collections.Generic;
using Bounce.Unmanaged;
using BepInEx.Configuration;
using Bounce.Singletons;
using System.Reflection;
using Newtonsoft.Json;

namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(FileAccessPlugin.Guid, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("org.hollofox.plugins.RadialUIPlugin", BepInDependency.DependencyFlags.SoftDependency)]
    public partial class ChatServicePlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Chat Service Plug-In";
        public const string Guid = "org.lordashes.plugins.chatservice";
        public const string Version = "2.4.5.0";

        public enum ChatSource
        {
            gm = 0,
            player = 1,
            creature = 2,
            hideVolume = 3,
            other = 888,
            anonymous = 999
        }

        public enum DiagnosticSelection
        {
            none = 0,
            low = 1,
            high = 2,
            ultra = 999,
            debug = 999
        }
        
        private static Dictionary<string, Func<string, string, Talespire.SourceRole, string>> chatMessgeServiceHandlers = new Dictionary<string, Func<string, string, Talespire.SourceRole, string>>();

        public static ConfigEntry<DiagnosticSelection> diagnostics;

        private static ConfigEntry<string> timestampStyle { get; set; }
        private static ConfigEntry<string> headerStyle { get; set; }
        private static ConfigEntry<string> contentStyle { get; set; }

        private static ConfigEntry<string> logFileNamePrefix { get; set; }
        private static ConfigEntry<string> ignoreChatServicesList { get; set; }

        private static object padlock = new object();

        private static string logFileName = "";

        private static Dictionary<string, string> aliases = new Dictionary<string, string>();

        /// <summary>
        /// Function for initializing plugin
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            diagnostics = Config.Bind("Settings", "Log Diagnostic Level", DiagnosticSelection.low);

            UnityEngine.Debug.Log("Chat Service Plugin: " + this.GetType().AssemblyQualifiedName + " Active. (Diagnostics=" + diagnostics.Value.ToString() + ")");

            timestampStyle = Config.Bind("Settings", "Timestamp Style", "font-size: 8pt; font-weight: normal; background-color: #454545;");
            headerStyle = Config.Bind("Settings", "Header Style", "font-size: 16pt; font-weight: bold; background-color: #555555;");
            contentStyle = Config.Bind("Settings", "Content Style", "font-size: 12pt; font-weight: normal; background-color: #656565;");
            logFileNamePrefix = Config.Bind("Settings", "Chat Log File Name (Or Empty For Off)", "");
            ignoreChatServicesList = Config.Bind("Settings", "List Of Chat Services To Not Log", "/org.lordashes.plugins.assetdata|");

            if (logFileNamePrefix.Value.Trim()!="")
            {
                logFileName = logFileNamePrefix.Value + "_" + DateTime.UtcNow.ToString("yyyy.MM.dd_HH.mm.ss") + ".html";
                System.IO.File.WriteAllText(logFileNamePrefix.Value.Trim(), "<HTML>\r\n  <BODY>\r\n");
            }

            var harmony = new Harmony(Guid);
            harmony.PatchAll();

            if (Config.Bind("Settings", "Add Deselect Option", true).Value == true)
            {
                if (diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: Creating Deselect Character Menu Option."); }
                MapMenu.ItemArgs args = new MapMenu.ItemArgs()
                {
                    Title = "Deselect",
                    Icon = FileAccessPlugin.Image.LoadSprite("Deselect.png"),
                    CloseMenuOnActivate = true,
                    Action = (mmi, obj) =>
                    {
                        if (diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: Deselecting All Assets"); }
                        foreach (CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets)
                        {
                            asset.Deselect();
                        }
                        if (diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: Updating LocalClient SelectedCreatureId"); }
                        foreach(PropertyInfo pi in typeof(LocalClient).GetProperties())
                        {
                            if (pi.Name == "SelectedCreatureId")
                            {
                                if (diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: Found SelectedCreatureId. Updating..."); }
                                pi.SetValue(null, CreatureGuid.Empty);
                            }
                            else
                            {
                                if (diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: Found '"+pi.Name+"'"); }
                            }
                        }                        
                        if (diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: Updating ChatInputBoardTool _selectedCreature"); }
                        ChatInputBoardTool __instance = GameObject.FindObjectOfType<ChatInputBoardTool>();
                        try { PatchAssistant.SetField(__instance, "_selectedCreature", null); } catch {; }
                        SetSpeaker();
                    }
                };
                if(SDIM.InvokeMethod("HolloFox_TS-RadialUIPlugin/RadialUI.dll", "AddCustomButtonOnCharacter", new object[] { ChatServicePlugin.Guid, args, null })==SDIM.InvokeResult.success)
                {
                    if (diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: Adding Deselect Character Menu Option."); }
                }
                else
                {
                    if (diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: RadialUI Plugin Not Available"); }
                }

                if(FileAccessPlugin.File.Exists("Chat_Service_Aliases.json"))
                {
                    aliases = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(FileAccessPlugin.File.ReadAllText("Chat_Service_Aliases.json"));
                    if (diagnostics.Value >= DiagnosticSelection.high)
                    {
                        foreach (KeyValuePair<string, string> alias in aliases)
                        {
                            Debug.Log("Chat Service Plugin: Adding Alias '/"+alias.Key+"' => '/"+alias.Value+"'");
                        }
                    }
                }
            }
        }

        public static void SetSpeaker()
        {
            try
            {
                string speaker = "Someone";
                CreatureBoardAsset asset = null;
                if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: Selected Characeter = "+Convert.ToString(LocalClient.SelectedCreatureId)); }
                CreaturePresenter.TryGetAsset(LocalClient.SelectedCreatureId, out asset);
                if (asset != null)
                {
                    if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: Setting Speaker Via Character Name"); }
                    speaker = asset.Name;
                    if (speaker.Contains("<")) { speaker = speaker.Substring(0, speaker.IndexOf("<")); }
                }
                else
                {
                    if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.ultra) { Debug.Log("Chat Service Plugin: Setting Speaker Via Player Name"); }
                    speaker = CampaignSessionManager.GetPlayerName(LocalPlayer.Id);
                }
                ChatInputBoardTool cibt = GameObject.FindObjectOfType<ChatInputBoardTool>();
                if (cibt != null)
                {
                    UIChatInputField uicif = (UIChatInputField)PatchAssistant.GetField(cibt, "_input");
                    if (uicif != null)
                    {
                        uicif.UpdateSpeaker(speaker);
                        if (ChatServicePlugin.diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: Speaker Set To " + speaker); }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public static void LogChatMessage(string creatureName, string chatMessage, string directive = "")
        {
            if (chatMessage.Trim() != "")
            {
                string msg = "    <DIV Width=480 Style=\"Width: 480px; border-style: solid;\">\r\n";
                msg = msg + "      <DIV Width=480 Id=Header Style=\"Width: 480px;" + timestampStyle.Value + "\">&nbsp;&nbsp;&nbsp;" + DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss") + "</DIV>\r\n";
                msg = msg + "      <DIV Width=480 Id=Header Style=\"Width: 480px;" + headerStyle.Value + "\">&nbsp;&nbsp;&nbsp;" + Utility.GetCreatureName(creatureName) + "</DIV>\r\n";
                if (directive != "")
                {
                    msg = msg + "      <DIV Width=480 Id=Header Style=\"Width: 480px;" + timestampStyle.Value + "\">&nbsp;&nbsp;&nbsp;Operation: " + directive + "</DIV>\r\n";
                }
                msg = msg + "      <DIV Width=480 Id=Content Style=\"Width: 480px;" + contentStyle.Value + "\">&nbsp;&nbsp;&nbsp;" + chatMessage + "</DIV>\r\n";
                msg = msg + "    </DIV><BR>\r\n";
                lock (padlock)
                {
                    System.IO.File.AppendAllText(logFileName, msg);
                }
            }
        }

        public static void LogEventMessage(string title, string message, string directive = "")
        {
            if (message.Trim() != "")
            {
                string msg = "    <DIV Width=480 Style=\"Width: 480px; border-style: solid;\">\r\n";
                msg = msg + "      <DIV Width=480 Id=Header Style=\"Width: 480px;" + timestampStyle.Value + "\">&nbsp;&nbsp;&nbsp;" + DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss") + "</DIV>\r\n";
                msg = msg + "      <DIV Width=480 Id=Header Style=\"Width: 480px;" + headerStyle.Value + "\">&nbsp;&nbsp;&nbsp;" + Utility.GetCreatureName(title) + "</DIV>\r\n";
                if (directive != "")
                {
                    msg = msg + "      <DIV Width=480 Id=Header Style=\"Width: 480px;" + timestampStyle.Value + "\">&nbsp;&nbsp;&nbsp;Operation: " + directive + "</DIV>\r\n";
                }
                msg = msg + "      <DIV Width=480 Id=Content Style=\"Width: 480px;" + contentStyle.Value + "\">&nbsp;&nbsp;&nbsp;" + message + "</DIV>\r\n";
                msg = msg + "    </DIV><BR>\r\n";
                lock (padlock)
                {
                    System.IO.File.AppendAllText(logFileName, msg);
                }
            }
        }

        public static void LogDiceResult(DiceManager.RollResults diceResult, ClientGuid sender, bool hidden)
        {
            DiceManager.RollResultsOperation op;
            DiceManager.RollResult result;
            DiceManager.RollValue value;

            string roll = "";
            int dice = 0;
            int sides = 0;
            int mod = 0;
            int multiplier = 0;

            int total = 0;
            int masterTotal = 0;

            PlayerGuid roller;
            int rollerIndex;
            BoardSessionManager.ClientsPlayerGuids.TryGetValueIndex(sender, out roller, out rollerIndex);

            string msg = "    <DIV Width=480 Style=\"Width: 480px; border-style: solid;\">\r\n";
            msg = msg + "      <DIV Width=480 Id=Header Style=\"Width: 480px;" + timestampStyle.Value + "\">&nbsp;&nbsp;&nbsp;" + DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss") + "</DIV>\r\n";
            msg = msg + "      <DIV Width=480 Id=Header Style=\"Width: 480px;" + headerStyle.Value + "\">&nbsp;&nbsp;&nbsp;" + Utility.GetCreatureName(CampaignSessionManager.GetPlayerName(roller)) + "</DIV>\r\n";

            int groupCount = 0;
            foreach (DiceManager.RollResultsGroup dgrd in diceResult.ResultsGroups)
            {
                groupCount++;
                dice = 0;
                sides = 0;
                mod = 0;
                multiplier = 0;

                if (Convert.ToString(dgrd.Name).Trim() != "")
                {
                    msg = msg + "      <DIV Width=480 Id=Header Style=\"Width: 480px;" + Utility.GetCreatureName(headerStyle.Value) + "\">&nbsp;&nbsp;&nbsp;" + dgrd.Name + "</DIV>\r\n";
                }

                dgrd.Result.Get(out op, out result, out value);

                multiplier = (op.Operator == DiceManager.DiceOperator.Add) ? +1 : -1;

                total = 0;

                foreach (DiceManager.RollOperand operand in op.Operands)
                {
                    operand.Get(out op, out result, out value);

                    if (result.Kind.RegisteredName == "<unknown>")
                    {
                        // Modifier
                        mod = (value.Value * multiplier);
                    }
                    else
                    {
                        // Dice
                        dice = result.Results.Length;
                        sides = int.Parse(result.Kind.RegisteredName.Substring(1));
                        roll = dice.ToString() + "D" + sides.ToString() + "{#}=[";
                        foreach (short rollValue in result.Results)
                        {
                            roll = roll + rollValue.ToString() + ",";
                            total = total + rollValue;
                        }
                        roll = roll.Substring(0, roll.Length - 1) + "]";
                    }
                }
                total = total + mod;
                masterTotal = masterTotal + total;
                roll = roll.Replace("{#}", ((mod >= 0) ? "+" : "") + mod);
                msg = msg + "      <DIV Width=480 Id=Header Style=\"Width: 480px;" + contentStyle.Value + "\">&nbsp;&nbsp;&nbsp;" + roll + "="+total+"</DIV>\r\n";
            }
            if (groupCount > 1)
            {
                msg = msg + "      <DIV Width=480 Id=Header Style=\"Width: 480px;" + contentStyle.Value + "\">&nbsp;&nbsp;&nbsp;<B>Total: " + masterTotal + "</b></DIV>\r\n";
            }
            msg = msg + "    </DIV><BR>\r\n";
            lock (padlock)
            {
                System.IO.File.AppendAllText(logFileName, msg);
            }
        }

        public static class ChatMessageService
        {
            public static void AddHandler(string key, Func<string, string, Talespire.SourceRole, string> callback)
            {
                if (diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: Added Subscription To " + key); }
                chatMessgeServiceHandlers.Add(key, callback);
            }

            public static void RemoveHandler(string key)
            {
                if (diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: Removed Subscription To " + key); }
                chatMessgeServiceHandlers.Remove(key);
            }

            public static bool CheckHandler(string key)
            {
                if (diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: Check If Handler '" + key+"' Is Registered"); }
                return chatMessgeServiceHandlers.ContainsKey(key);
            }

            public static void SendMessage(string message, NGuid source)
            {
                if (diagnostics.Value >= DiagnosticSelection.high) { Debug.Log("Chat Service Plugin: Sending " + message + " (signature " + source + ")"); }
                ChatManager.SendChatMessageToBoard(message, source);
            }
        }
    }
}
