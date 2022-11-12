using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenWorldServer.Services;

namespace OpenWorldServer
{
    public static class ConsoleUtils
    {
        public static void UpdateTitle() => Console.Title = $"OpenWorld {Server.serverVersion} - {StaticProxy.serverConfig.ServerName} | {StaticProxy.playerManager.ConnectedClients.Count} / {StaticProxy.serverConfig.MaxPlayers} Players";

        public enum ConsoleLogMode
        {
            Title,
            Heading,
            Normal,
            Info,
            Warning,
            Error,
            Done
        }

        private static readonly Dictionary<ConsoleLogMode, ConsoleColor> ConsoleLogColors = new Dictionary<ConsoleLogMode, ConsoleColor>()
        {
            { ConsoleLogMode.Heading, ConsoleColor.White},
            { ConsoleLogMode.Normal, ConsoleColor.Gray},
            { ConsoleLogMode.Info, ConsoleColor.Blue},
            { ConsoleLogMode.Warning, ConsoleColor.Yellow},
            { ConsoleLogMode.Error, ConsoleColor.Red},
            { ConsoleLogMode.Done, ConsoleColor.DarkGreen}
        };


        public static void LogTitleToConsole(string title)
            => ConsoleUtils.LogToConsole($"== {title} ==", ConsoleColor.Cyan);

        private static void LogToConsole(string data, ConsoleColor textColor)
        {
            Console.ForegroundColor = textColor;
            ConsoleUtils.LogToConsole(data);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void LogToConsole(string data, ConsoleLogMode mode = ConsoleLogMode.Normal)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                string[] lines = data.Split('\n');
                if (mode == ConsoleLogMode.Heading)
                {
                    lines = lines.Prepend(new string('-', lines.Max(x => x.Length))).Append(new string('-', lines.Max(x => x.Length))).ToArray();
                }
                string formattedData = string.Join('\n', lines.Select(x => $"[{DateTime.Now:HH:mm:ss}] | {x}"));

                // Reset to the left margin to overwrite our "Enter Command>" prompt.
                Console.SetCursorPosition(0, Console.CursorTop);
                // Set the color to use for the entry (defaults to DEFAULT_COLOR if not passed in as an arg).
                Console.ForegroundColor = ConsoleLogColors[mode];

                Console.WriteLine(formattedData);

                if (data.StartsWith("Chat - [")) LogToFile(formattedData, FileLogMode.Chat);
                else if (data.StartsWith("Gift Done Between")) LogToFile(formattedData, FileLogMode.Gift);
                else if (data.StartsWith("Trade Done Between")) LogToFile(formattedData, FileLogMode.Trade);
                else if (data.StartsWith("Barter Done Between")) LogToFile(formattedData, FileLogMode.Barter);
                else if (data.StartsWith("Spy Done Between")) LogToFile(formattedData, FileLogMode.Spy);
                else if (data.StartsWith("PvP Done Between")) LogToFile(formattedData, FileLogMode.PvP);
                else if (data.StartsWith("Visit Done Between")) LogToFile(formattedData, FileLogMode.Visit);
                else LogToFile(formattedData, FileLogMode.General);

                Console.ForegroundColor = Console.ForegroundColor = ConsoleLogColors[ConsoleLogMode.Normal];
                Console.Write("Command> ");
            }
        }

        public enum FileLogMode
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
        public static void LogToFile(string data, FileLogMode mode = FileLogMode.General)
        {
            // Year-Month-Day is always superior because chronological=alphabetical.
            string pathToday = Path.Combine(PathProvider.LogsFolderPath, DateTime.Today.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(pathToday)) Directory.CreateDirectory(pathToday);
            Dictionary<FileLogMode, string> files = new Dictionary<FileLogMode, string>()
            {
                { FileLogMode.Chat, "chat.log" },
                { FileLogMode.Gift, "gift.log" },
                { FileLogMode.Trade, "trade.log" },
                { FileLogMode.Barter, "barter.log" },
                { FileLogMode.Spy, "spy.log" },
                { FileLogMode.PvP, "pvp.log" },
                { FileLogMode.Visit, "visit.log" },
                { FileLogMode.General, "log.log" },
                { FileLogMode.WarningError, "warning_error.log" }
            };
            try { File.AppendAllText(pathToday + Path.DirectorySeparatorChar + files[mode], $"{data}\n"); }
            catch
            {
                // This can't use LogToConsole as it will cause an infinite loop, potentially.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR WRITING LOG FILE: {pathToday + Path.DirectorySeparatorChar + files[mode]}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}
