using System;
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
            client.PlayerData.Username = data.Split('│')[1].ToLower();
            client.PlayerData.Password = data.Split('│')[2];

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

            if (!CheckIfUserExisted(client)) PlayerUtils.SaveNewPlayerFile(client.PlayerData.Username, client.PlayerData.Password);
            else if (!CheckForPassword(client)) return;

            ConsoleUtils.UpdateTitle();
            ServerUtils.SendPlayerListToAll(client);

            CheckForJoinMode(client, joinMode);
        }

        private static void CheckForJoinMode(ServerClient client, string joinMode)
        {
            if (joinMode == "NewGame")
            {
                SendNewGameData(client);
                ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] Has Reset Game Progress");
            }

            else if (joinMode == "LoadGame")
            {
                PlayerUtils.GiveSavedDataToPlayer(client);
                SendLoadGameData(client);
            }

            ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] " + "[" +
                ((IPEndPoint)client.tcp.Client.RemoteEndPoint).Address.ToString() + "] " + "Has Connected");
        }

        private static void SendNewGameData(ServerClient client)
        {
            //We give saved data back to return data that is not removed at new creation
            PlayerUtils.GiveSavedDataToPlayer(client);
            PlayerUtils.SaveNewPlayerFile(client.PlayerData.Username, client.PlayerData.Password);

            Networking.SendData(client, GetPlanetToSend());
            Thread.Sleep(100);

            string settlementsToSend = GetSettlementsToSend(client);
            Networking.SendData(client, settlementsToSend);
            Thread.Sleep(100);

            Networking.SendData(client, GetVariablesToSend(client));
            Thread.Sleep(100);

            ServerUtils.SendPlayerList(client);
            Thread.Sleep(100);

            Networking.SendData(client, FactionHandler.GetFactionDetails(client));
            Thread.Sleep(100);

            Networking.SendData(client, FactionBuildingHandler.GetAllFactionStructures(client));
            Thread.Sleep(100);

            Networking.SendData(client, "NewGame│");
        }

        private static void SendLoadGameData(ServerClient client)
        {
            string settlementsToSend = GetSettlementsToSend(client);
            Networking.SendData(client, settlementsToSend);
            Thread.Sleep(100);

            Networking.SendData(client, GetVariablesToSend(client));
            Thread.Sleep(100);

            ServerUtils.SendPlayerList(client);
            Thread.Sleep(100);

            Networking.SendData(client, FactionHandler.GetFactionDetails(client));
            Thread.Sleep(100);

            Networking.SendData(client, FactionBuildingHandler.GetAllFactionStructures(client));
            Thread.Sleep(100);

            Networking.SendData(client, GetGiftsToSend(client));
            Thread.Sleep(100);

            Networking.SendData(client, GetTradesToSend(client));
            Thread.Sleep(100);

            Networking.SendData(client, "LoadGame│");
        }

        private static bool CheckIfUserExisted(ServerClient client)
        {
            ServerClient clientToFetch = Server.savedClients.Find(fetch => fetch.PlayerData.Username == client.PlayerData.Username);

            if (clientToFetch == null) return false;
            else return true;
        }

        private static bool CheckForPassword(ServerClient client)
        {
            ServerClient clientToFetch = Server.savedClients.Find(fetch => fetch.PlayerData.Username == client.PlayerData.Username);

            client.PlayerData.Username = clientToFetch.PlayerData.Username;

            if (clientToFetch.PlayerData.Password != client.PlayerData.Password)
            {
                Networking.SendData(client, "Disconnect│WrongPassword");

                client.disconnectFlag = true;

                ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] Has Been Kicked For: [Wrong Password]");

                return false;
            }

            return true;
        }

        public static string GetPlanetToSend()
        {
            string dataToSend = "Planet│";

            double mmGC = Server.globeCoverage;
            string mmS = Server.seed;
            int mmOR = Server.overallRainfall;
            int mmOT = Server.overallTemperature;
            int mmOP = Server.overallPopulation;

            return dataToSend + mmGC + "│" + mmS + "│" + mmOR + "│" + mmOT + "│" + mmOP;
        }

        public static string GetSettlementsToSend(ServerClient client)
        {
            string dataToSend = "Settlements│";

            if (Server.savedSettlements.Count == 0) return dataToSend;

            else
            {
                foreach (KeyValuePair<string, List<string>> pair in Server.savedSettlements)
                {
                    if (pair.Value[0] == client.PlayerData.Username) continue;

                    int factionValue = 0;

                    ServerClient clientToCompare = Server.savedClients.Find(fetch => fetch.PlayerData.Username == pair.Value[0]);
                    if (client.PlayerData.Faction == null) factionValue = 0;
                    if (clientToCompare.PlayerData.Faction == null) factionValue = 0;
                    else if (client.PlayerData.Faction != null && clientToCompare.PlayerData.Faction != null)
                    {
                        if (client.PlayerData.Faction.name == clientToCompare.PlayerData.Faction.name) factionValue = 1;
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

            if (Server.savedClients.Find(fetch => fetch.PlayerData.Username == client.PlayerData.Username) != null)
            {
                client.PlayerData.IsAdmin = Server.savedClients.Find(fetch => fetch.PlayerData.Username == client.PlayerData.Username).PlayerData.IsAdmin;
            }
            else client.PlayerData.IsAdmin = false;

            int devInt = client.PlayerData.IsAdmin || Server.allowDevMode ? 1 : 0;

            int wipeInt = client.PlayerData.ToWipe ? 1 : 0;

            int roadInt = 0;
            if (Server.usingRoadSystem) roadInt = 1;
            if (Server.usingRoadSystem && Server.aggressiveRoadMode) roadInt = 2;

            string name = Server.serverName;

            int chatInt = Server.usingChat ? 1 : 0;

            int profanityInt = Server.usingProfanityFilter ? 1 : 0;

            int modVerifyInt = Server.usingModVerification ? 1 : 0;

            return dataToSend + devInt + "│" + wipeInt + "│" + roadInt + "│" + chatInt + "│" + profanityInt + "│" + modVerifyInt + "│" + name;
        }

        public static string GetGiftsToSend(ServerClient client)
        {
            string dataToSend = "GiftedItems│";

            if (client.PlayerData.GiftString.Count == 0) return dataToSend;

            else
            {
                string giftsToSend = "";

                foreach (string str in client.PlayerData.GiftString) giftsToSend += str + "│";

                dataToSend += giftsToSend;

                client.PlayerData.GiftString.Clear();

                return dataToSend;
            }
        }

        public static string GetTradesToSend(ServerClient client)
        {
            string dataToSend = "TradedItems│";

            if (client.PlayerData.TradeString.Count == 0) return dataToSend;

            else
            {
                string tradesToSend = "";

                foreach (string str in client.PlayerData.TradeString) tradesToSend += str + "│";

                dataToSend += tradesToSend;

                client.PlayerData.TradeString.Clear();

                return dataToSend;
            }
        }

        public static bool CompareModsWithClient(ServerClient client, string data)
        {
            if (client.PlayerData.IsAdmin) return true;
            if (!Server.forceModlist) return true;

            string[] clientMods = data.Split('»');

            string flaggedMods = "";

            bool flagged = false;

            foreach (string clientMod in clientMods)
            {
                if (Server.whitelistedMods.Contains(clientMod)) continue;
                else if (Server.blacklistedMods.Contains(clientMod))
                {
                    flagged = true;
                    flaggedMods += clientMod + "»";
                }
                else if (!Server.enforcedMods.Contains(clientMod))
                {
                    flagged = true;
                    flaggedMods += clientMod + "»";
                }
            }

            foreach (string serverMod in Server.enforcedMods)
            {
                if (!clientMods.Contains(serverMod))
                {
                    flagged = true;
                    flaggedMods += serverMod + "»";
                }
            }

            if (flagged)
            {
                ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] " + "Doesn't Have The Required Mod Or Mod Files Mismatch!");
                flaggedMods = flaggedMods.Remove(flaggedMods.Count() - 1, 1);
                Networking.SendData(client, "Disconnect│WrongMods│" + flaggedMods);

                client.disconnectFlag = true;
                return false;
            }
            else return true;
        }

        public static bool CompareConnectingClientWithConnecteds(ServerClient client)
        {
            foreach (ServerClient sc in Networking.connectedClients)
            {
                if (sc.PlayerData.Username == client.PlayerData.Username)
                {
                    if (sc == client) continue;

                    Networking.SendData(sc, "Disconnect│AnotherLogin");
                    sc.disconnectFlag = true;
                    return false;
                }
            }

            return true;
        }

        public static bool CompareConnectingClientWithWhitelist(ServerClient client)
        {
            if (!Server.usingWhitelist) return true;
            if (client.PlayerData.IsAdmin) return true;

            foreach (string str in Server.whitelistedUsernames)
            {
                if (str == client.PlayerData.Username) return true;
            }

            Networking.SendData(client, "Disconnect│Whitelist");
            client.disconnectFlag = true;
            ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] Tried To Join But Is Not Whitelisted");
            return false;
        }

        public static bool CompareConnectingClientVersion(ServerClient client, string clientVersion)
        {
            string latestVersion = "";

            try
            {
                WebClient wc = new WebClient();
                latestVersion = wc.DownloadString("https://raw.githubusercontent.com/TastyLollipop/OpenWorld/main/Latest%20Versions%20Cache");
                latestVersion = latestVersion.Split('│')[2].Replace("- Latest Client Version: ", "");
                latestVersion = latestVersion.Remove(0, 1);
                latestVersion = latestVersion.Remove(latestVersion.Count() - 1, 1);
            }
            catch { return true; }

            if (clientVersion == latestVersion) return true;
            else
            {
                Networking.SendData(client, "Disconnect│Version");
                client.disconnectFlag = true;
                ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] Tried To Join But Is Using Other Version");
                return false;
            }
        }

        public static bool CompareClientIPWithBans(ServerClient client)
        {
            foreach (KeyValuePair<string, string> pair in Server.bannedIPs)
            {
                if (pair.Key == ((IPEndPoint)client.tcp.Client.RemoteEndPoint).Address.ToString() || pair.Value == client.PlayerData.Username)
                {
                    Networking.SendData(client, "Disconnect│Banned");
                    client.disconnectFlag = true;
                    ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] Tried To Join But Is Banned");
                    return false;
                }
            }

            return true;
        }

        public static bool CompareConnectingClientWithPlayerCount(ServerClient client)
        {
            if (client.PlayerData.IsAdmin) return true;

            if (Networking.connectedClients.Count() >= Server.maxPlayers + 1)
            {
                Networking.SendData(client, "Disconnect│ServerFull");
                client.disconnectFlag = true;
                return false;
            }

            return true;
        }

        public static bool ParseClientUsername(ServerClient client)
        {
            if (string.IsNullOrWhiteSpace(client.PlayerData.Username))
            {
                Networking.SendData(client, "Disconnect│Corrupted");
                client.disconnectFlag = true;
                return false;
            }

            if (!client.PlayerData.Username.All(character => Char.IsLetterOrDigit(character) || character == '_' || character == '-'))
            {
                Networking.SendData(client, "Disconnect│Corrupted");
                client.disconnectFlag = true;
                return false;
            }

            else return true;
        }
    }
}
