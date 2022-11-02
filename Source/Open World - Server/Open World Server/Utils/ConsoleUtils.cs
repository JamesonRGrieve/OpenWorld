using System;
using System.Collections.Generic;
using System.IO;
using OpenWorldServer.Services;

namespace OpenWorldServer
{
    public static class ConsoleUtils
    {
        public static void UpdateTitle()
        {
            Console.Title = StaticProxy.serverConfig.ServerName + " " + Server.serverVersion + " / " + Networking.connectedClients.Count + " Of " + StaticProxy.serverConfig.MaxPlayers + " Connected Players";
        }

        public static void LogTitleToConsole(string title)
            => ConsoleUtils.LogToConsole($"== {title} ==", ConsoleColor.Blue);

        public static void LogToConsole(string data, ConsoleColor textColor)
        {
            Console.ForegroundColor = textColor;
            ConsoleUtils.LogToConsole(data);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void LogToConsole(string data)
        {
            string dataToLog = "";
            if (data != Environment.NewLine) dataToLog = "[" + DateTime.Now + "]" + " │ " + data;
            else dataToLog = "";

            Console.WriteLine(dataToLog);

            if (data.StartsWith("Chat - [")) WriteToLog(dataToLog, LogMode.Chat);
            else if (data.StartsWith("Gift Done Between")) WriteToLog(dataToLog, LogMode.Gift);
            else if (data.StartsWith("Trade Done Between")) WriteToLog(dataToLog, LogMode.Trade);
            else if (data.StartsWith("Barter Done Between")) WriteToLog(dataToLog, LogMode.Barter);
            else if (data.StartsWith("Spy Done Between")) WriteToLog(dataToLog, LogMode.Spy);
            else if (data.StartsWith("PvP Done Between")) WriteToLog(dataToLog, LogMode.PvP);
            else if (data.StartsWith("Visit Done Between")) WriteToLog(dataToLog, LogMode.Visit);
            else WriteToLog(dataToLog, LogMode.General);
        }

        public static void WriteWithTime(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) Console.WriteLine();
            else Console.WriteLine("[" + DateTime.Now + "] | " + str);
        }

        public enum LogMode
        {
            Chat,
            Gift,
            Trade,
            Barter,
            Spy,
            PvP,
            Visit,
            General,
            WarningError
        }

        public static void WriteToLog(string data, LogMode mode = LogMode.General)
        {
            // Year-Month-Day is always superior because chronological=alphabetical.
            string pathToday = Path.Combine(PathProvider.LogsFolderPath, DateTime.Today.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(pathToday)) Directory.CreateDirectory(pathToday);

            Dictionary<LogMode, string> files = new Dictionary<LogMode, string>()
            {
                { LogMode.Chat, "chat.log" },
                { LogMode.Gift, "gift.log" },
                { LogMode.Trade, "trade.log" },
                { LogMode.Barter, "barter.log" },
                { LogMode.Spy, "spy.log" },
                { LogMode.PvP, "pvp.log" },
                { LogMode.Visit, "visit.log" },
                { LogMode.General, "log.log" },
                { LogMode.WarningError, "warning_error.log" }
            };

            try { File.AppendAllText(pathToday + Path.DirectorySeparatorChar + files[mode], $"{data}\n"); }
            catch { }
        }

        public static void DisplayNetworkStatus()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            LogToConsole("Network Check:");

            Console.ForegroundColor = ConsoleColor.White;
            LogToConsole("Server Started");
            LogToConsole("Type 'Help' To See Available Commands");
            LogToConsole("Network Line Started");
            Console.WriteLine("");
        }
    }
}
