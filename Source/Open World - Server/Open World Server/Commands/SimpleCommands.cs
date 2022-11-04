using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenWorldServer.Handlers.Old;

namespace OpenWorldServer
{
    public static class SimpleCommands
    {
        //Miscellaneous

        public static void HelpCommand()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("List Of Available Commands:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Help - Displays Help Menu");
            ConsoleUtils.WriteWithTime("Settings - Displays Settings Menu");
            ConsoleUtils.WriteWithTime("Modlist - Displays Mods Menu");
            ConsoleUtils.WriteWithTime("List - Displays Player List Menu");
            ConsoleUtils.WriteWithTime("Whitelist - Shows All Whitelisted Players");
            ConsoleUtils.WriteWithTime("Settlements - Displays Settlements Menu");
            ConsoleUtils.WriteWithTime("Faction - Displays All Data About X Faction");
            ConsoleUtils.WriteWithTime("Reload - Reloads All Available Settings Into The Server");
            ConsoleUtils.WriteWithTime("Status - Shows A General Overview Menu");
            ConsoleUtils.WriteWithTime("Clear - Clears The Console");
            ConsoleUtils.WriteWithTime("Exit - Closes The Server");
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Communication:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Say - Send A Chat Message");
            ConsoleUtils.WriteWithTime("Broadcast - Send A Letter To Every Player Connected");
            ConsoleUtils.WriteWithTime("Notify - Send A Letter To X Player");
            ConsoleUtils.WriteWithTime("Chat - Displays Chat Menu");
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Interaction:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Invoke - Invokes An Event To X Player");
            ConsoleUtils.WriteWithTime("Plague - Invokes An Event To All Connected Players");
            ConsoleUtils.WriteWithTime("Eventlist - Shows All Available Events");
            ConsoleUtils.WriteWithTime("GiveItem - Gives An Item To X Player");
            ConsoleUtils.WriteWithTime("GiveItemAll - Gives An Item To All Players");
            ConsoleUtils.WriteWithTime("Protect - Protects A Player From Any Event Temporarily");
            ConsoleUtils.WriteWithTime("Deprotect - Disables All Protections Given To X Player");
            ConsoleUtils.WriteWithTime("Immunize - Protects A Player From Any Event Permanently");
            ConsoleUtils.WriteWithTime("Deimmunize - Disables The Immunity Given To X Player");
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Admin Control:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Player - Displays All Data About X Player");
            ConsoleUtils.WriteWithTime("Promote - Promotes X Player To Admin");
            ConsoleUtils.WriteWithTime("Demote - Demotes X Player");
            ConsoleUtils.WriteWithTime("Adminlist - Shows All Server Admins");
            ConsoleUtils.WriteWithTime("Kick - Kicks X Player");
            ConsoleUtils.WriteWithTime("Ban - Bans X Player");
            ConsoleUtils.WriteWithTime("Pardon - Pardons X Player");
            ConsoleUtils.WriteWithTime("Banlist - Shows All Banned Players");
            ConsoleUtils.WriteWithTime("Wipe - Deletes Every Player Data In The Server");

            Console.WriteLine("");
        }

        public static void SettingsCommand()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Server Settings:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Server Name: " + StaticProxy.serverConfig.ServerName);
            ConsoleUtils.WriteWithTime("Server Description: " + StaticProxy.serverConfig.Description);
            ConsoleUtils.WriteWithTime("Server Local IP: " + Networking.localAddress);
            ConsoleUtils.WriteWithTime("Server Port: " + Networking.serverPort);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("World Settings:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Globe Coverage: " + StaticProxy.serverConfig.World.GlobeCoverage);
            ConsoleUtils.WriteWithTime("Seed: " + StaticProxy.serverConfig.World.Seed);
            ConsoleUtils.WriteWithTime("Overall Rainfall: " + StaticProxy.serverConfig.World.OverallRainfall);
            ConsoleUtils.WriteWithTime("Overall Temperature: " + StaticProxy.serverConfig.World.OverallTemperature);
            ConsoleUtils.WriteWithTime("Overall Population: " + StaticProxy.serverConfig.World.OverallPopulation);
            Console.WriteLine("");
        }

        public static void ModListCommand()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Server Enforced Mods: " + StaticProxy.modHandler.RequiredMods.Length);
            Console.ForegroundColor = ConsoleColor.White;

            if (StaticProxy.modHandler.RequiredMods.Length == 0)
                ConsoleUtils.WriteWithTime("No Enforced Mods Found");
            else
                foreach (var modMetaData in StaticProxy.modHandler.RequiredMods)
                    ConsoleUtils.WriteWithTime(modMetaData.Name);

            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Server Whitelisted Mods: " + StaticProxy.modHandler.WhitelistedMods.Length);
            Console.ForegroundColor = ConsoleColor.White;

            if (StaticProxy.modHandler.WhitelistedMods.Length == 0)
                ConsoleUtils.WriteWithTime("No Whitelisted Mods Found");
            else
                foreach (var modMetaData in StaticProxy.modHandler.WhitelistedMods)
                    ConsoleUtils.WriteWithTime(modMetaData.Name);

            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Server Blacklisted Mods: " + StaticProxy.modHandler.BlacklistedMods.Length);
            Console.ForegroundColor = ConsoleColor.White;

            if (StaticProxy.modHandler.BlacklistedMods.Length == 0)
                ConsoleUtils.WriteWithTime("No Blacklisted Mods Found");
            else
                foreach (var modMetaData in StaticProxy.modHandler.BlacklistedMods)
                    ConsoleUtils.WriteWithTime(modMetaData.Name);
            Console.WriteLine("");
        }

        public static void ExitCommand()
        {
            PlayerClient[] clientsToKick = Networking.connectedClients.ToArray();
            foreach (PlayerClient sc in clientsToKick)
            {
                Networking.SendData(sc, "Disconnect│Closing");
                sc.IsDisconnecting = true;
            }

            Server.exit = true;
        }

        public static void ClearCommand()
        {
            Console.Clear();
        }

        public static void ReloadCommand()
        {
            Console.Clear();

            StaticProxy.modHandler.ReloadModFolders();
            Console.ForegroundColor = ConsoleColor.Green;

            FactionHandler.CheckFactions(false);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("");

            PlayerUtils.CheckAllAvailablePlayers(false);
            Console.ForegroundColor = ConsoleColor.Green;
        }

        public static void StatusCommand()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Server Status");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Version: " + Server.serverVersion);
            ConsoleUtils.WriteWithTime("Connection: Online");
            ConsoleUtils.WriteWithTime("Uptime: " + (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()));
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Mods:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Enforced Mods: " + StaticProxy.modHandler.RequiredMods.Length);
            ConsoleUtils.WriteWithTime("Whitelisted Mods: " + StaticProxy.modHandler.WhitelistedMods.Length);
            ConsoleUtils.WriteWithTime("Blacklisted Mods: " + StaticProxy.modHandler.BlacklistedMods.Length);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Players:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Connected Players: " + Networking.connectedClients.Count);
            ConsoleUtils.WriteWithTime("Saved Players: " + Server.savedClients.Count);
            ConsoleUtils.WriteWithTime("Saved Settlements: " + Server.savedSettlements.Count);
            ConsoleUtils.WriteWithTime("Whitelisted Players: " + StaticProxy.playerHandler.WhitelistHandler.Whitelist.Count);
            ConsoleUtils.WriteWithTime("Max Players: " + StaticProxy.serverConfig.MaxPlayers);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Modlist Settings:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Using Modlist Check: " + StaticProxy.serverConfig.ModsSystem.MatchModlist);
            ConsoleUtils.WriteWithTime("Using Modlist Config Check: " + StaticProxy.serverConfig.ModsSystem.ModlistConfigMatch);
            ConsoleUtils.WriteWithTime("Using Mod Verification: " + StaticProxy.serverConfig.ModsSystem.ForceModVerification);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Chat Settings:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Using Chat: " + StaticProxy.serverConfig.ChatSystem.IsActive);
            ConsoleUtils.WriteWithTime("Using Profanity Filter: " + StaticProxy.serverConfig.ChatSystem.UseProfanityFilter);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Wealth Settings:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Using Wealth System: " + StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.IsActive);
            ConsoleUtils.WriteWithTime("Warning Threshold: " + StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.WarningThreshold);
            ConsoleUtils.WriteWithTime("Ban Threshold: " + StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.BanThreshold);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Idle Settings:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Using Idle System: " + StaticProxy.serverConfig.IdleSystem.IsActive);
            ConsoleUtils.WriteWithTime("Idle Threshold: " + StaticProxy.serverConfig.IdleSystem.IdleThresholdInDays);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Road Settings:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Using Road System: " + StaticProxy.serverConfig.RoadSystem.IsActive);
            ConsoleUtils.WriteWithTime("Aggressive Road Mode: " + StaticProxy.serverConfig.RoadSystem.AggressiveRoadMode);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Miscellaneous Settings");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Using Whitelist: " + StaticProxy.serverConfig.WhitelistMode);
            ConsoleUtils.WriteWithTime("Using Enforced Difficulty: " + StaticProxy.serverConfig.ForceDifficulty);
            ConsoleUtils.WriteWithTime("Allow Dev Mode: " + StaticProxy.serverConfig.AllowDevMode);

            Console.WriteLine("");
        }

        //Administration

        public static void WhiteListCommand()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Whitelisted Players: " + StaticProxy.playerHandler.WhitelistHandler.Whitelist.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (StaticProxy.playerHandler.WhitelistHandler.Whitelist.Count == 0) ConsoleUtils.WriteWithTime("No Whitelisted Players Found");
            else foreach (string str in StaticProxy.playerHandler.WhitelistHandler.Whitelist) ConsoleUtils.WriteWithTime("" + str);

            Console.WriteLine("");
        }

        //Check this one
        public static void AdminListCommand()
        {
            Console.Clear();

            Server.adminList.Clear();

            PlayerClient[] savedClients = Server.savedClients.ToArray();
            foreach (PlayerClient client in savedClients)
            {
                if (client.Account.IsAdmin) Server.adminList.Add(client.Account.Username);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Server Administrators: " + Server.adminList.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (Server.adminList.Count == 0) ConsoleUtils.WriteWithTime("No Administrators Found");
            else foreach (string str in Server.adminList) ConsoleUtils.WriteWithTime("" + str);

            Console.WriteLine("");
        }

        public static void BanListCommand()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Banned players: " + StaticProxy.playerHandler.BanlistHandler.Banlist.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (StaticProxy.playerHandler.BanlistHandler.Banlist.Count == 0)
                ConsoleUtils.WriteWithTime("No Banned Players");
            else
            {
                // ToDo: Use Copy of Dictionary
                foreach (var ban in StaticProxy.playerHandler.BanlistHandler.Banlist)
                {
                    ConsoleUtils.WriteWithTime("[" + ban.Username + "] - [" + ban.IPAddress + "]");
                }
            }

            Console.WriteLine("");
        }

        public static void WipeCommand()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Red;
            ConsoleUtils.WriteWithTime("WARNING! THIS ACTION WILL DELETE ALL PLAYER DATA. DO YOU WANT TO PROCEED? (Y/N)");
            Console.ForegroundColor = ConsoleColor.White;

            string response = Console.ReadLine();

            if (response == "Y")
            {
                PlayerClient[] clients = Networking.connectedClients.ToArray();
                foreach (PlayerClient client in clients)
                {
                    client.IsDisconnecting = true;
                }

                PlayerClient[] savedClients = Server.savedClients.ToArray();
                foreach (PlayerClient client in savedClients)
                {
                    client.Account.Wealth = 0;
                    client.Account.PawnCount = 0;
                    StaticProxy.playerHandler.AccountsHandler.SaveAccount(client);
                }

                Console.Clear();

                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleUtils.WriteWithTime("All Player Files Have Been Set To Wipe");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else Console.Clear();
        }

        //Player Interaction

        public static void ListCommand()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Connected Players: " + Networking.connectedClients.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (Networking.connectedClients.Count == 0)
                ConsoleUtils.WriteWithTime("No Players Connected");
            else
            {
                PlayerClient[] clients = Networking.connectedClients.ToArray();
                foreach (PlayerClient client in clients)
                {
                    try
                    {
                        ConsoleUtils.WriteWithTime("" + client.Account.Username);
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.WriteWithTime("Error Processing Player With IP " + client.IPAddress.ToString());
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }

            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Saved Players: " + Server.savedClients.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (Server.savedClients.Count == 0)
                ConsoleUtils.WriteWithTime("No Players Saved");
            else
            {
                PlayerClient[] savedClients = Server.savedClients.ToArray();
                foreach (PlayerClient savedClient in savedClients)
                {
                    try { ConsoleUtils.WriteWithTime("" + savedClient.Account.Username); }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.WriteWithTime("Error Processing Player With IP " + savedClient.IPAddress.ToString());
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }

            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Saved Factions: " + Server.savedFactions.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (Server.savedFactions.Count == 0)
                ConsoleUtils.WriteWithTime("No Factions Saved");
            else
            {
                Faction[] factions = Server.savedFactions.ToArray();
                foreach (Faction savedFaction in factions)
                {
                    ConsoleUtils.WriteWithTime(savedFaction.name);
                }
            }

            Console.WriteLine("");
        }

        public static void SettlementsCommand()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Server Settlements: " + Server.savedSettlements.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (Server.savedSettlements.Count == 0) ConsoleUtils.WriteWithTime("No Active Settlements");
            else
            {
                Dictionary<string, List<string>> settlements = Server.savedSettlements;
                foreach (KeyValuePair<string, List<string>> pair in settlements)
                {
                    ConsoleUtils.WriteWithTime("[" + pair.Key + "] - [" + pair.Value[0] + "]");
                }
            }

            Console.WriteLine("");
        }

        public static void ChatCommand()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Server Chat:");
            Console.ForegroundColor = ConsoleColor.White;

            if (Server.chatCache.Count == 0) ConsoleUtils.WriteWithTime("No Chat Messages");
            else
            {
                string[] chat = Server.chatCache.ToArray();
                foreach (string message in chat)
                {
                    ConsoleUtils.WriteWithTime(message);
                }
            }

            Console.WriteLine("");
        }

        public static void EventListCommand()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("List Of Available Events:");

            Console.ForegroundColor = ConsoleColor.White;
            ConsoleUtils.WriteWithTime("Raid");
            ConsoleUtils.WriteWithTime("Infestation");
            ConsoleUtils.WriteWithTime("MechCluster");
            ConsoleUtils.WriteWithTime("ToxicFallout");
            ConsoleUtils.WriteWithTime("Manhunter");
            ConsoleUtils.WriteWithTime("Wanderer");
            ConsoleUtils.WriteWithTime("FarmAnimals");
            ConsoleUtils.WriteWithTime("ShipChunk");
            ConsoleUtils.WriteWithTime("GiveQuest");
            ConsoleUtils.WriteWithTime("TraderCaravan");

            Console.WriteLine("");
        }

        //Unknown

        public static void UnknownCommand(string command)
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Command [" + command + "] Not Found");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("");
        }
    }
}