﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace OpenWorldServer
{
    public static class JoiningsUtils
    {
        public static void LoginProcedures(ServerClient client, string data)
        {
            client.username = data.Split('│')[1].ToLower();
            client.password = data.Split('│')[2];

            string playerVersion = data.Split('│')[3];
            string joinMode = data.Split('│')[4];
            string playerMods = data.Split('│')[5];

            if (!CompareConnectingClientWithPlayerCount(client)) return;
            if (!CompareConnectingClientVersion(client, playerVersion)) return;
            if (!CompareConnectingClientWithWhitelist(client)) return;
            if (!CompareModsWithClient(client, playerMods)) return;
            if (!CompareClientIPWithBans(client)) return;
            if (!ParseClientUsername(client)) return;
            CompareConnectingClientWithConnecteds(client);

            if (!CheckIfUserExisted(client)) PlayerUtils.SaveNewPlayerFile(client.username, client.password);
            else if (!CheckForPassword(client)) return;

            ConsoleUtils.UpdateTitle();
            ServerUtils.SendPlayerListToAll(client);

            CheckForJoinMode(client, joinMode);
        }

        private static void CheckForJoinMode(ServerClient client, string joinMode)
        {
            if (joinMode == "NewGame") SendNewGameData(client);

            else if (joinMode == "LoadGame")
            {
                PlayerUtils.GiveSavedDataToPlayer(client);
                SendLoadGameData(client);
            }

            ConsoleUtils.LogToConsole("Player [" + client.username + "] Has Connected");
        }

        private static void SendNewGameData(ServerClient client)
        {
            //We give saved data back to return data that is not removed at new creation
            PlayerUtils.GiveSavedDataToPlayer(client);
            PlayerUtils.SaveNewPlayerFile(client.username, client.password);

            Networking.SendData(client, GetPlanetToSend());

            Networking.SendData(client, GetSettlementsToSend(client));

            Networking.SendData(client, GetVariablesToSend(client));

            ServerUtils.SendPlayerList(client);

            Networking.SendData(client, FactionHandler.GetFactionDetails(client));

            Networking.SendData(client, FactionBuildingHandler.GetAllFactionStructures(client));

            Networking.SendData(client, "NewGame│");
        }

        private static void SendLoadGameData(ServerClient client)
        {
            Networking.SendData(client, GetSettlementsToSend(client));

            Networking.SendData(client, GetVariablesToSend(client));

            ServerUtils.SendPlayerList(client);

            Networking.SendData(client, FactionHandler.GetFactionDetails(client));

            Networking.SendData(client, FactionBuildingHandler.GetAllFactionStructures(client));

            Networking.SendData(client, GetGiftsToSend(client));

            Networking.SendData(client, GetTradesToSend(client));

            Networking.SendData(client, "LoadGame│");
        }

        private static bool CheckIfUserExisted(ServerClient client)
        {
            ServerClient clientToFetch = OpenWorldServer.savedClients.Find(fetch => fetch.username == client.username);

            if (clientToFetch == null) return false;
            else return true;
        }

        private static bool CheckForPassword(ServerClient client)
        {
            ServerClient clientToFetch = OpenWorldServer.savedClients.Find(fetch => fetch.username == client.username);

            client.username = clientToFetch.username;

            if (clientToFetch.password != client.password)
            {
                Networking.SendData(client, "Disconnect│WrongPassword");

                client.disconnectFlag = true;

                ConsoleUtils.LogToConsole("Player [" + client.username + "] Has Been Kicked For: [Wrong Password]");

                return false;
            }

            return true;
        }

        public static string GetPlanetToSend()
        {
            string dataToSend = "Planet│";

            float mmGC = OpenWorldServer.globeCoverage;
            string mmS = OpenWorldServer.seed;
            int mmOR = OpenWorldServer.overallRainfall;
            int mmOT = OpenWorldServer.overallTemperature;
            int mmOP = OpenWorldServer.overallPopulation;

            return dataToSend + mmGC + "│" + mmS + "│" + mmOR + "│" + mmOT + "│" + mmOP;
        }

        public static string GetSettlementsToSend(ServerClient client)
        {
            string dataToSend = "Settlements│";

            if (OpenWorldServer.savedSettlements.Count == 0) return dataToSend;

            else
            {
                Dictionary<string, List<string>> settlements = OpenWorldServer.savedSettlements;
                foreach (KeyValuePair<string, List<string>> pair in settlements)
                {
                    if (pair.Value[0] == client.username) continue;

                    int factionValue = 0;

                    ServerClient clientToCompare = OpenWorldServer.savedClients.Find(fetch => fetch.username == pair.Value[0]);
                    if (client.faction == null) factionValue = 0;
                    if (clientToCompare.faction == null) factionValue = 0;
                    else if (client.faction != null && clientToCompare.faction != null)
                    {
                        if (client.faction.name == clientToCompare.faction.name) factionValue = 1;
                        else factionValue = 2;
                    }

                    dataToSend += pair.Key + ":" + pair.Value[0] + ":" + factionValue + "│";
                }

                return dataToSend;
            }
        }

        public static string GetVariablesToSend(ServerClient client)
        {
            string dataToSend = "Variables│";

            if (OpenWorldServer.savedClients.Find(fetch => fetch.username == client.username) != null)
            {
                client.isAdmin = OpenWorldServer.savedClients.Find(fetch => fetch.username == client.username).isAdmin;
            }
            else client.isAdmin = false;

            int devInt = client.isAdmin || OpenWorldServer.allowDevMode ? 1 : 0;

            int wipeInt = client.toWipe ? 1 : 0;

            int roadInt = 0;
            if (OpenWorldServer.usingRoadSystem) roadInt = 1;
            if (OpenWorldServer.usingRoadSystem && OpenWorldServer.aggressiveRoadMode) roadInt = 2;

            int chatInt = OpenWorldServer.usingChat ? 1 : 0;

            int profanityInt = OpenWorldServer.usingProfanityFilter ? 1 : 0;

            int modVerifyInt = OpenWorldServer.usingModVerification ? 1 : 0;

            int enforcedDifficultyInt = OpenWorldServer.usingEnforcedDifficulty ? 1 : 0;

            string name = OpenWorldServer.serverName;

            return dataToSend + devInt + "│" + wipeInt + "│" + roadInt + "│" + chatInt + "│" + profanityInt + "│" + modVerifyInt + "│" + enforcedDifficultyInt + "│" + name;
        }

        public static string GetGiftsToSend(ServerClient client)
        {
            string dataToSend = "GiftedItems│";

            if (client.giftString.Count == 0) return dataToSend;

            else
            {
                string giftsToSend = "";

                foreach (string str in client.giftString) giftsToSend += str + "»";

                dataToSend += giftsToSend;

                client.giftString.Clear();

                return dataToSend;
            }
        }

        public static string GetTradesToSend(ServerClient client)
        {
            string dataToSend = "TradedItems│";

            if (client.tradeString.Count == 0) return dataToSend;

            else
            {
                string tradesToSend = "";

                foreach (string str in client.tradeString) tradesToSend += str + "»";

                dataToSend += tradesToSend;

                client.tradeString.Clear();

                return dataToSend;
            }
        }

        public static bool CompareModsWithClient(ServerClient client, string data)
        {
            if (client.isAdmin) return true;
            if (!OpenWorldServer.forceModlist) return true;

            string[] clientMods = data.Split('»');

            string flaggedMods = "";

            bool flagged = false;

            foreach (string clientMod in clientMods)
            {
                if (OpenWorldServer.whitelistedMods.Contains(clientMod)) continue;
                else if (OpenWorldServer.blacklistedMods.Contains(clientMod))
                {
                    flagged = true;
                    flaggedMods += clientMod + "»";
                }
                else if (!OpenWorldServer.enforcedMods.Contains(clientMod))
                {
                    flagged = true;
                    flaggedMods += clientMod + "»";
                }
            }

            foreach (string serverMod in OpenWorldServer.enforcedMods)
            {
                if (!clientMods.Contains(serverMod))
                {
                    flagged = true;
                    flaggedMods += serverMod + "»";
                }
            }

            if (flagged)
            {
                flaggedMods = flaggedMods.Remove(flaggedMods.Count() - 1, 1);

                Networking.SendData(client, "Disconnect│WrongMods│" + flaggedMods);
                client.disconnectFlag = true;
                ConsoleUtils.LogToConsole("Player [" + client.username + "] " + "Doesn't Have The Required Mod Or Mod Files Mismatch!");
                return false;
            }
            else return true;
        }

        public static void CompareConnectingClientWithConnecteds(ServerClient client)
        {
            ServerClient[] clients = Networking.connectedClients.ToArray();
            foreach (ServerClient sc in clients)
            {
                if (sc.username == client.username)
                {
                    if (sc == client) continue;

                    Networking.SendData(sc, "Disconnect│AnotherLogin");
                    sc.disconnectFlag = true;
                    return;
                }
            }
        }

        public static bool CompareConnectingClientWithWhitelist(ServerClient client)
        {
            if (!OpenWorldServer.usingWhitelist) return true;
            if (client.isAdmin) return true;

            foreach (string str in OpenWorldServer.whitelistedUsernames)
            {
                if (str == client.username) return true;
            }

            Networking.SendData(client, "Disconnect│Whitelist");
            client.disconnectFlag = true;
            ConsoleUtils.LogToConsole("Player [" + client.username + "] Tried To Join But Is Not Whitelisted");
            return false;
        }

        public static bool CompareConnectingClientVersion(ServerClient client, string clientVersion)
        {
            if (string.IsNullOrWhiteSpace(OpenWorldServer.latestClientVersion)) return true;

            if (clientVersion == OpenWorldServer.latestClientVersion) return true;
            else
            {
                Networking.SendData(client, "Disconnect│Version");
                client.disconnectFlag = true;
                ConsoleUtils.LogToConsole("Player [" + client.username + "] Tried To Join But Is Using Other Version");
                return false;
            }
        }

        public static bool CompareClientIPWithBans(ServerClient client)
        {
            Dictionary<string, string> bannedIPs = OpenWorldServer.bannedIPs;
            foreach (KeyValuePair<string, string> pair in bannedIPs)
            {
                if (pair.Key == ((IPEndPoint)client.tcp.Client.RemoteEndPoint).Address.ToString() || pair.Value == client.username)
                {
                    Networking.SendData(client, "Disconnect│Banned");
                    client.disconnectFlag = true;
                    ConsoleUtils.LogToConsole("Player [" + client.username + "] Tried To Join But Is Banned");
                    return false;
                }
            }

            return true;
        }

        public static bool CompareConnectingClientWithPlayerCount(ServerClient client)
        {
            if (client.isAdmin) return true;

            if (Networking.connectedClients.Count() >= OpenWorldServer.maxPlayers + 1)
            {
                Networking.SendData(client, "Disconnect│ServerFull");
                client.disconnectFlag = true;
                return false;
            }

            return true;
        }

        public static bool ParseClientUsername(ServerClient client)
        {
            if (string.IsNullOrWhiteSpace(client.username))
            {
                Networking.SendData(client, "Disconnect│Corrupted");
                client.disconnectFlag = true;
                return false;
            }

            if (!client.username.All(character => Char.IsLetterOrDigit(character) || character == '_' || character == '-'))
            {
                Networking.SendData(client, "Disconnect│Corrupted");
                client.disconnectFlag = true;
                return false;
            }

            else return true;
        }
    }
}
