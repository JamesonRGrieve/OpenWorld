using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;

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

            ConsoleUtils.WriteWithTime("Server Name: " + OpenWorldServer.serverName);
            ConsoleUtils.WriteWithTime("Server Description: " + OpenWorldServer.serverDescription);
            ConsoleUtils.WriteWithTime("Server Local IP: " + Networking.localAddress);
            ConsoleUtils.WriteWithTime("Server Port: " + Networking.serverPort);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("World Settings:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Globe Coverage: " + OpenWorldServer.globeCoverage);
            ConsoleUtils.WriteWithTime("Seed: " + OpenWorldServer.seed);
            ConsoleUtils.WriteWithTime("Overall Rainfall: " + OpenWorldServer.overallRainfall);
            ConsoleUtils.WriteWithTime("Overall Temperature: " + OpenWorldServer.overallTemperature);
            ConsoleUtils.WriteWithTime("Overall Population: " + OpenWorldServer.overallPopulation);
            Console.WriteLine("");
        }

        public static void ModListCommand()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Server Enforced Mods: " + OpenWorldServer.enforcedMods.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (OpenWorldServer.enforcedMods.Count == 0) ConsoleUtils.WriteWithTime("No Enforced Mods Found");
            else foreach (string mod in OpenWorldServer.enforcedMods) ConsoleUtils.WriteWithTime(mod);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Server Whitelisted Mods: " + OpenWorldServer.whitelistedMods.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (OpenWorldServer.whitelistedMods.Count == 0) ConsoleUtils.WriteWithTime("No Whitelisted Mods Found");
            else foreach (string whitelistedMod in OpenWorldServer.whitelistedMods) ConsoleUtils.WriteWithTime(whitelistedMod);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Server Blacklisted Mods: " + OpenWorldServer.blacklistedMods.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (OpenWorldServer.whitelistedMods.Count == 0) ConsoleUtils.WriteWithTime("No Blacklisted Mods Found");
            else foreach (string blacklistedMod in OpenWorldServer.blacklistedMods) ConsoleUtils.WriteWithTime(blacklistedMod);
            Console.WriteLine("");
        }

        public static void ExitCommand()
        {
            ServerClient[] clientsToKick = Networking.connectedClients.ToArray();
            foreach (ServerClient sc in clientsToKick)
            {
                Networking.SendData(sc, "Disconnect│Closing");
                sc.disconnectFlag = true;
            }

            OpenWorldServer.exit = true;
        }

        public static void ClearCommand()
        {
            Console.Clear();
        }

        public static void ReloadCommand()
        {
            Console.Clear();

            ModHandler.CheckMods(false);
            Console.ForegroundColor = ConsoleColor.Green;

            WorldHandler.CheckWorldFile();
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

            ConsoleUtils.WriteWithTime("Version: " + OpenWorldServer.serverVersion);
            ConsoleUtils.WriteWithTime("Connection: Online");
            ConsoleUtils.WriteWithTime("Uptime: " + (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()));
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Mods:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Enforced Mods: " + OpenWorldServer.enforcedMods.Count);
            ConsoleUtils.WriteWithTime("Whitelisted Mods: " + OpenWorldServer.whitelistedMods.Count);
            ConsoleUtils.WriteWithTime("Blacklisted Mods: " + OpenWorldServer.blacklistedMods.Count);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Players:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Connected Players: " + Networking.connectedClients.Count);
            ConsoleUtils.WriteWithTime("Saved Players: " + OpenWorldServer.savedClients.Count);
            ConsoleUtils.WriteWithTime("Saved Settlements: " + OpenWorldServer.savedSettlements.Count);
            ConsoleUtils.WriteWithTime("Whitelisted Players: " + OpenWorldServer.whitelistedUsernames.Count);
            ConsoleUtils.WriteWithTime("Max Players: " + OpenWorldServer.maxPlayers);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Modlist Settings:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Using Modlist Check: " + OpenWorldServer.forceModlist);
            ConsoleUtils.WriteWithTime("Using Modlist Config Check: " + OpenWorldServer.forceModlistConfigs);
            ConsoleUtils.WriteWithTime("Using Mod Verification: " + OpenWorldServer.usingModVerification);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Chat Settings:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Using Chat: " + OpenWorldServer.usingChat);
            ConsoleUtils.WriteWithTime("Using Profanity Filter: " + OpenWorldServer.usingProfanityFilter);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Wealth Settings:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Using Wealth System: " + OpenWorldServer.usingWealthSystem);
            ConsoleUtils.WriteWithTime("Warning Threshold: " + OpenWorldServer.warningWealthThreshold);
            ConsoleUtils.WriteWithTime("Ban Threshold: " + OpenWorldServer.banWealthThreshold);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Idle Settings:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Using Idle System: " + OpenWorldServer.usingIdleTimer);
            ConsoleUtils.WriteWithTime("Idle Threshold: " + OpenWorldServer.idleTimer);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Road Settings:");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Using Road System: " + OpenWorldServer.usingRoadSystem);
            ConsoleUtils.WriteWithTime("Aggressive Road Mode: " + OpenWorldServer.aggressiveRoadMode);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Miscellaneous Settings");
            Console.ForegroundColor = ConsoleColor.White;

            ConsoleUtils.WriteWithTime("Using Whitelist: " + OpenWorldServer.usingWhitelist);
            ConsoleUtils.WriteWithTime("Using Enforced Difficulty: " + OpenWorldServer.usingEnforcedDifficulty);
            ConsoleUtils.WriteWithTime("Allow Dev Mode: " + OpenWorldServer.allowDevMode);

            Console.WriteLine("");
        }

        //Administration

        public static void WhiteListCommand()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Whitelisted Players: " + OpenWorldServer.whitelistedUsernames.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (OpenWorldServer.whitelistedUsernames.Count == 0) ConsoleUtils.WriteWithTime("No Whitelisted Players Found");
            else foreach (string str in OpenWorldServer.whitelistedUsernames) ConsoleUtils.WriteWithTime("" + str);

            Console.WriteLine("");
        }

        //Check this one
        public static void AdminListCommand()
        {
            Console.Clear();

            OpenWorldServer.adminList.Clear();

            ServerClient[] savedClients = OpenWorldServer.savedClients.ToArray();
            foreach (ServerClient client in savedClients)
            {
                if (client.isAdmin) OpenWorldServer.adminList.Add(client.username);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Server Administrators: " + OpenWorldServer.adminList.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (OpenWorldServer.adminList.Count == 0) ConsoleUtils.WriteWithTime("No Administrators Found");
            else foreach (string str in OpenWorldServer.adminList) ConsoleUtils.WriteWithTime("" + str);

            Console.WriteLine("");
        }

        public static void BanListCommand()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Banned players: " + OpenWorldServer.bannedIPs.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (OpenWorldServer.bannedIPs.Count == 0) ConsoleUtils.WriteWithTime("No Banned Players");
            else
            {
                Dictionary<string, string> bannedIPs = OpenWorldServer.bannedIPs;
                foreach (KeyValuePair<string, string> pair in bannedIPs)
                {
                    ConsoleUtils.WriteWithTime("[" + pair.Value + "] - [" + pair.Key + "]");
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
                ServerClient[] clients = Networking.connectedClients.ToArray();
                foreach (ServerClient client in clients)
                {
                    client.disconnectFlag = true;
                }

                ServerClient[] savedClients = OpenWorldServer.savedClients.ToArray();
                foreach (ServerClient client in savedClients)
                {
                    client.wealth = 0;
                    client.pawnCount = 0;
                    PlayerUtils.SavePlayer(client);
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

            if (Networking.connectedClients.Count == 0) ConsoleUtils.WriteWithTime("No Players Connected");
            else
            {
                ServerClient[] clients = Networking.connectedClients.ToArray();
                foreach (ServerClient client in clients)
                {
                    try { ConsoleUtils.WriteWithTime("" + client.username); }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.WriteWithTime("Error Processing Player With IP " + ((IPEndPoint)client.tcp.Client.RemoteEndPoint).Address.ToString());
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }

            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Saved Players: " + OpenWorldServer.savedClients.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (OpenWorldServer.savedClients.Count == 0) ConsoleUtils.WriteWithTime("No Players Saved");
            else
            {
                ServerClient[] savedClients = OpenWorldServer.savedClients.ToArray();
                foreach (ServerClient savedClient in savedClients)
                {
                    try { ConsoleUtils.WriteWithTime("" + savedClient.username); }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.WriteWithTime("Error Processing Player With IP " + ((IPEndPoint)savedClient.tcp.Client.RemoteEndPoint).Address.ToString());
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }

            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Saved Factions: " + OpenWorldServer.savedFactions.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (OpenWorldServer.savedFactions.Count == 0) ConsoleUtils.WriteWithTime("No Factions Saved");
            else
            {
                Faction[] factions = OpenWorldServer.savedFactions.ToArray();
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
            ConsoleUtils.WriteWithTime("Server Settlements: " + OpenWorldServer.savedSettlements.Count);
            Console.ForegroundColor = ConsoleColor.White;

            if (OpenWorldServer.savedSettlements.Count == 0) ConsoleUtils.WriteWithTime("No Active Settlements");
            else
            {
                Dictionary<string, List<string>> settlements = OpenWorldServer.savedSettlements;
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

            if (OpenWorldServer.chatCache.Count == 0) ConsoleUtils.WriteWithTime("No Chat Messages");
            else
            {
                string[] chat = OpenWorldServer.chatCache.ToArray();
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