using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace OpenWorldServer
{
    public static class ServerUtils
    {
        public static void SetCulture()
        {
            Console.ForegroundColor = ConsoleColor.White;
            ConsoleUtils.LogToConsole("Using Culture Info: [" + CultureInfo.CurrentCulture + "]");

            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", false);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US", false);
        }

        public static void SetPaths()
        {
            OpenWorldServer.mainFolderPath = AppDomain.CurrentDomain.BaseDirectory;

            OpenWorldServer.logFolderPath = OpenWorldServer.mainFolderPath + Path.DirectorySeparatorChar + "Logs";
            OpenWorldServer.serverSettingsPath = OpenWorldServer.mainFolderPath + Path.DirectorySeparatorChar + "Server Settings.txt";
            OpenWorldServer.worldSettingsPath = OpenWorldServer.mainFolderPath + Path.DirectorySeparatorChar + "World Settings.txt";
            OpenWorldServer.playersFolderPath = OpenWorldServer.mainFolderPath + Path.DirectorySeparatorChar + "Players";
            OpenWorldServer.factionsFolderPath = OpenWorldServer.mainFolderPath + Path.DirectorySeparatorChar + "Factions";
            OpenWorldServer.enforcedModsFolderPath = OpenWorldServer.mainFolderPath + Path.DirectorySeparatorChar + "Enforced Mods";
            OpenWorldServer.whitelistedModsFolderPath = OpenWorldServer.mainFolderPath + Path.DirectorySeparatorChar + "Whitelisted Mods";
            OpenWorldServer.blacklistedModsFolderPath = OpenWorldServer.mainFolderPath + Path.DirectorySeparatorChar + "Blacklisted Mods";
            OpenWorldServer.whitelistedUsersPath = OpenWorldServer.mainFolderPath + Path.DirectorySeparatorChar + "Whitelisted Players.txt";

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Server Startup:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.LogToConsole("Base Directory At: [" + OpenWorldServer.mainFolderPath + "]");
        }

        public static void CheckServerVersion()
        {
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Server Version Check:");
            Console.ForegroundColor = ConsoleColor.White;

            string latestVersion = "";

            try
            {
                WebClient wc = new WebClient();
                latestVersion = wc.DownloadString("https://raw.githubusercontent.com/TastyLollipop/OpenWorld/main/Latest%20Versions%20Cache");
                latestVersion = latestVersion.Split('│')[1].Replace("- Latest Server Version: ", "");
                latestVersion = latestVersion.Remove(0, 1);
                latestVersion = latestVersion.Remove(latestVersion.Count() - 1, 1);
            }

            catch
            {
                Console.ForegroundColor = ConsoleColor.White;
                ConsoleUtils.LogToConsole("Version Check Failed. This Is Not Dangerous");
                Console.ForegroundColor = ConsoleColor.White;
            }

            if (OpenWorldServer.serverVersion == latestVersion) ConsoleUtils.LogToConsole("Running Latest Version");
            else ConsoleUtils.LogToConsole("Running Outdated Or Unstable version. Please Update From Github At Earliest Convenience To Prevent Errors");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void CheckClientVersionRequirement()
        {
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Client Version Check:");
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                string version;

                WebClient wc = new WebClient();
                version = wc.DownloadString("https://raw.githubusercontent.com/TastyLollipop/OpenWorld/main/Latest%20Versions%20Cache");
                version = version.Split('│')[2].Replace("- Latest Client Version: ", "");
                version = version.Remove(0, 1);
                version = version.Remove(version.Count() - 1, 1);

                OpenWorldServer.latestClientVersion = version;

                ConsoleUtils.LogToConsole("Listening For Version [" + OpenWorldServer.latestClientVersion + "]");
            }

            catch
            {
                Console.ForegroundColor = ConsoleColor.White;
                ConsoleUtils.LogToConsole("Version Check Failed. This Is Not Dangerous");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void CheckSettingsFile()
        {
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Settings Check:");
            Console.ForegroundColor = ConsoleColor.White;

            if (File.Exists(OpenWorldServer.serverSettingsPath))
            {
                string[] settings = File.ReadAllLines(OpenWorldServer.serverSettingsPath);

                foreach(string setting in settings)
                {
                    if (setting.StartsWith("Server Name: "))
                    {
                        string splitString = setting.Replace("Server Name: ", "");
                        OpenWorldServer.serverName = splitString;
                        continue;
                    }

                    else if (setting.StartsWith("Server Description: "))
                    {
                        string splitString = setting.Replace("Server Description: ", "");
                        OpenWorldServer.serverDescription = splitString;
                        continue;
                    }

                    else if (setting.StartsWith("Server Local IP: "))
                    {
                        string splitString = setting.Replace("Server Local IP: ", "");
                        Networking.localAddress = IPAddress.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Server Port: "))
                    {
                        string splitString = setting.Replace("Server Port: ", "");
                        Networking.serverPort = int.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Max Players: "))
                    {
                        string splitString = setting.Replace("Max Players: ", "");
                        OpenWorldServer.maxPlayers = int.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Allow Dev Mode: "))
                    {
                        string splitString = setting.Replace("Allow Dev Mode: ", "");

                        if (splitString == "True") OpenWorldServer.allowDevMode = true;

                        continue;
                    }

                    else if (setting.StartsWith("Use Whitelist: "))
                    {
                        string splitString = setting.Replace("Use Whitelist: ", "");

                        if (splitString == "True") OpenWorldServer.usingWhitelist = true;

                        continue;
                    }

                    else if (setting.StartsWith("Use Enforced Difficulty: "))
                    {
                        string splitString = setting.Replace("Use Enforced Difficulty: ", "");

                        if (splitString == "True") OpenWorldServer.usingEnforcedDifficulty = true;

                        continue;
                    }

                    else if (setting.StartsWith("Wealth Warning Threshold: "))
                    {
                        string splitString = setting.Replace("Wealth Warning Threshold: ", "");
                        OpenWorldServer.warningWealthThreshold = int.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Wealth Ban Threshold: "))
                    {
                        string splitString = setting.Replace("Wealth Ban Threshold: ", "");
                        OpenWorldServer.banWealthThreshold = int.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Use Wealth System: "))
                    {
                        string splitString = setting.Replace("Use Wealth System: ", "");
                        if (splitString == "True")
                        {
                            OpenWorldServer.usingWealthSystem = true;
                        }
                        else if (splitString == "False")
                        {
                            OpenWorldServer.usingWealthSystem = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Use Idle System: "))
                    {
                        string splitString = setting.Replace("Use Idle System: ", "");
                        if (splitString == "True")
                        {
                            OpenWorldServer.usingIdleTimer = true;
                        }
                        else if (splitString == "False")
                        {
                            OpenWorldServer.usingIdleTimer = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Idle Threshold (days): "))
                    {
                        string splitString = setting.Replace("Idle Threshold (days): ", "");
                        OpenWorldServer.idleTimer = int.Parse(splitString);
                        continue;
                    }

                    else if (setting.StartsWith("Use Road System: "))
                    {
                        string splitString = setting.Replace("Use Road System: ", "");
                        if (splitString == "True")
                        {
                            OpenWorldServer.usingRoadSystem = true;
                        }
                        else if (splitString == "False")
                        {
                            OpenWorldServer.usingRoadSystem = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Aggressive Road Mode (WIP): "))
                    {
                        string splitString = setting.Replace("Aggressive Road Mode (WIP): ", "");
                        if (splitString == "True")
                        {
                            OpenWorldServer.aggressiveRoadMode = true;
                        }
                        else if (splitString == "False")
                        {
                            OpenWorldServer.aggressiveRoadMode = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Use Modlist Match: "))
                    {
                        string splitString = setting.Replace("Use Modlist Match: ", "");
                        if (splitString == "True")
                        {
                            OpenWorldServer.forceModlist = true;
                        }
                        else if (splitString == "False")
                        {
                            OpenWorldServer.forceModlist = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Use Modlist Config Match (WIP): "))
                    {
                        string splitString = setting.Replace("Use Modlist Config Match (WIP): ", "");
                        if (splitString == "True")
                        {
                            OpenWorldServer.forceModlistConfigs = true;
                        }
                        else if (splitString == "False")
                        {
                            OpenWorldServer.forceModlistConfigs = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Use Mod Verification: "))
                    {
                        string splitString = setting.Replace("Use Mod Verification: ", "");
                        if (splitString == "True")
                        {
                            OpenWorldServer.usingModVerification = true;
                        }
                        else if (splitString == "False")
                        {
                            OpenWorldServer.usingModVerification = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Use Chat: "))
                    {
                        string splitString = setting.Replace("Use Chat: ", "");
                        if (splitString == "True")
                        {
                            OpenWorldServer.usingChat = true;
                        }
                        else if (splitString == "False")
                        {
                            OpenWorldServer.usingChat = false;
                        }
                        continue;
                    }

                    else if (setting.StartsWith("Use Profanity filter: "))
                    {
                        string splitString = setting.Replace("Use Profanity filter: ", "");
                        if (splitString == "True")
                        {
                            OpenWorldServer.usingProfanityFilter = true;
                        }
                        else if (splitString == "False")
                        {
                            OpenWorldServer.usingProfanityFilter = false;
                        }
                        continue;
                    }
                }

                ConsoleUtils.LogToConsole("Loaded Settings File");
            }

            else
            {
                string[] settingsPreset = new string[]
                {
                    "- Server Details -",
                    "Server Name: My Server Name",
                    "Server Description: My Server Description",
                    "Server Local IP: 0.0.0.0",
                    "Server Port: 25555",
                    "Max Players: 300",
                    "Allow Dev Mode: False",
                    "Use Whitelist: False",
                    "Use Enforced Difficulty: False",
                    "",
                    "- Mod System Details -",
                    "Use Modlist Match: True",
                    "Use Modlist Config Match (WIP): False",
                    "Force Mod Verification: False",
                    "",
                    "- Chat System Details -",
                    "Use Chat: True",
                    "Use Profanity filter: True",
                    "",
                    "- Wealth System Details -",
                    "Use Wealth System: False",
                    "Wealth Warning Threshold: 10000",
                    "Wealth Ban Threshold: 100000",
                    "",
                    "- Idle System Details -",
                    "Use Idle System: True",
                    "Idle Threshold (days): 7",
                    "",
                    "- Road System Details -",
                    "Use Road System: True",
                    "Aggressive Road Mode (WIP): False",
                };

                File.WriteAllLines(OpenWorldServer.serverSettingsPath, settingsPreset);

                ConsoleUtils.LogToConsole("Generating Settings File");

                CheckSettingsFile();
            }
        }

        public static void SendChatMessage(ServerClient client, string data)
        {
            string message = data.Split('│')[2];

            string messageForConsole = "Chat - [" + client.username + "] " + message;

            ConsoleUtils.LogToConsole(messageForConsole);

            OpenWorldServer.chatCache.Add("[" + DateTime.Now + "]" + " │ " + messageForConsole);

            ServerClient[] allConnectedClients = Networking.connectedClients.ToArray();
            foreach (ServerClient sc in allConnectedClients)
            {
                if (sc == client) continue;
                else Networking.SendData(sc, data);
            }
        }

        public static void SendPlayerListToAll(ServerClient client)
        {
            ServerClient[] allConnectedClients = Networking.connectedClients.ToArray();
            foreach (ServerClient sc in allConnectedClients)
            {
                if (sc == client) continue;

                SendPlayerList(sc);
            }
        }

        public static void SendPlayerList(ServerClient client)
        {
            string playersToSend = GetPlayersToSend(client);
            Networking.SendData(client, playersToSend);
        }

        public static string GetPlayersToSend(ServerClient client)
        {
            string dataToSend = "PlayerList│";

            if (Networking.connectedClients.Count == 0) return dataToSend;

            else
            {
                ServerClient[] allConnectedClients = Networking.connectedClients.ToArray();
                foreach (ServerClient sc in allConnectedClients)
                {
                    if (sc == client) continue;

                    else dataToSend += sc.username + ":";
                }

                dataToSend += "│" + Networking.connectedClients.Count();

                return dataToSend;
            }
        }
    }
}