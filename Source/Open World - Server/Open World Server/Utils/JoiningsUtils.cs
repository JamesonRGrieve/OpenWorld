using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using OpenWorldServer.Enums;

namespace OpenWorldServer
{
    public static class JoiningsUtils
    {
        public static void LoginProcedures(PlayerClient client, string data)
        {
            client.PlayerData.Username = data.Split('│')[1].ToLower();
            client.PlayerData.Password = data.Split('│')[2];

            string playerVersion = data.Split('│')[3];
            string joinMode = data.Split('│')[4];
            string playerMods = data.Split('│')[5];

            if (!CompareConnectingClientWithPlayerCount(client)) return;
            if (!CompareConnectingClientVersion(client, playerVersion)) return;

            if (!StaticProxy.playerHandler.IsWhitelisted(client))
            {
                Networking.SendData(client, "Disconnect│Whitelist");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] tried to Join but is not Whitelisted");
            }

            if (!CompareModsWithClient(client, playerMods)) return;
            if (!CompareClientIPWithBans(client)) return;
            if (!ParseClientUsername(client)) return;
            CompareConnectingClientWithConnecteds(client);


            var playerData = StaticProxy.playerHandler.GetPlayerData(client);
            if (playerData == null)
            {
                StaticProxy.playerHandler.SavePlayerData(client);
                ConsoleUtils.LogToConsole("New Player [" + client.PlayerData.Username + "]");
            }
            else if (playerData.Password != client.PlayerData.Password)
            {
                Networking.SendData(client, "Disconnect│WrongPassword");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] has been Kicked for: [Wrong Password]");
                return;
            }

            ConsoleUtils.UpdateTitle();
            ServerUtils.SendPlayerListToAll(client);

            CheckForJoinMode(client, ParseJoinMode(joinMode));
        }

        private static JoinMode ParseJoinMode(string joinMode)
        {
            // We cant use this since a typo could be a bigger problem.
            // When the protocol is changed to send a byte for the JoinMode, we can use it to parse by casting.
            // return (JoinMode)Enum.Parse(typeof(JoinMode), joinMode);

            switch (joinMode)
            {
                case "NewGame":
                    return JoinMode.NewGame;
                case "LoadGame":
                    return JoinMode.LoadGame;
                default:
                    throw new ArgumentException($"JoinMode '{joinMode}' is not a vaild JoinMode");
            }
        }

        private static void CheckForJoinMode(PlayerClient client, JoinMode joinMode)
        {
            if (joinMode == JoinMode.NewGame)
			{
                ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] has started a new game");
            }
            else if (joinMode == JoinMode.LoadGame)
            {
                PlayerUtils.GiveSavedDataToPlayer(client);
                SendLoadGameData(client);
            }

            ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] has Connected");
        }

        private static void SendNewGameData(PlayerClient client)
        {
            //We give saved data back to return data that is not removed at new creation
            PlayerUtils.GiveSavedDataToPlayer(client);
            StaticProxy.playerHandler.ResetPlayerData(client, true);

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

        private static void SendLoadGameData(PlayerClient client)
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

        public static string GetPlanetToSend()
        {
            string dataToSend = "Planet│";

            double mmGC = StaticProxy.serverConfig.World.GlobeCoverage;
            string mmS = StaticProxy.serverConfig.World.Seed;
            int mmOR = StaticProxy.serverConfig.World.OverallRainfall;
            int mmOT = StaticProxy.serverConfig.World.OverallTemperature;
            int mmOP = StaticProxy.serverConfig.World.OverallPopulation;

            return dataToSend + mmGC + "│" + mmS + "│" + mmOR + "│" + mmOT + "│" + mmOP;
        }

        public static string GetSettlementsToSend(PlayerClient client)
        {
            string dataToSend = "Settlements│";

            if (Server.savedSettlements.Count == 0) return dataToSend;

            else
            {
                Dictionary<string, List<string>> settlements = Server.savedSettlements;
                foreach (KeyValuePair<string, List<string>> pair in settlements)
                {
                    if (pair.Value[0] == client.PlayerData.Username) continue;

                    int factionValue = 0;

                    PlayerClient clientToCompare = Server.savedClients.Find(fetch => fetch.PlayerData.Username == pair.Value[0]);
                    if (client.PlayerData.Faction == null)
                        factionValue = 0;
                    if (clientToCompare.PlayerData.Faction == null)
                        factionValue = 0;
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

        public static string GetVariablesToSend(PlayerClient client)
        {
            string dataToSend = "Variables│";

            if (Server.savedClients.Find(fetch => fetch.PlayerData.Username == client.PlayerData.Username) != null)
            {
                client.PlayerData.IsAdmin = Server.savedClients.Find(fetch => fetch.PlayerData.Username == client.PlayerData.Username).PlayerData.IsAdmin;
            }
            else client.PlayerData.IsAdmin = false;

            int devInt = client.PlayerData.IsAdmin || StaticProxy.serverConfig.AllowDevMode ? 1 : 0;

            int wipeInt = client.PlayerData.ToWipe ? 1 : 0;

            int roadInt = 0;
            if (StaticProxy.serverConfig.RoadSystem.IsActive) roadInt = 1;
            if (StaticProxy.serverConfig.RoadSystem.IsActive && StaticProxy.serverConfig.RoadSystem.AggressiveRoadMode) roadInt = 2;

            string name = StaticProxy.serverConfig.ServerName;

            int chatInt = StaticProxy.serverConfig.ChatSystem.IsActive ? 1 : 0;

            int profanityInt = StaticProxy.serverConfig.ChatSystem.UseProfanityFilter ? 1 : 0;

            int modVerifyInt = StaticProxy.serverConfig.ModsSystem.ForceModVerification ? 1 : 0;

            int enforcedDifficultyInt = StaticProxy.serverConfig.ForceDifficulty ? 1 : 0;

            return dataToSend + devInt + "│" + wipeInt + "│" + roadInt + "│" + chatInt + "│" + profanityInt + "│" + modVerifyInt + "│" + enforcedDifficultyInt + "│" + name;
        }

        public static string GetGiftsToSend(PlayerClient client)
        {
            string dataToSend = "GiftedItems│";

            if (client.PlayerData.GiftString.Count == 0) return dataToSend;

            else
            {
                string giftsToSend = "";

                foreach (string str in client.PlayerData.GiftString) giftsToSend += str + "»";

                dataToSend += giftsToSend;

                client.PlayerData.GiftString.Clear();

                return dataToSend;
            }
        }

        public static string GetTradesToSend(PlayerClient client)
        {
            string dataToSend = "TradedItems│";

            if (client.PlayerData.TradeString.Count == 0) return dataToSend;

            else
            {
                string tradesToSend = "";

                foreach (string str in client.PlayerData.TradeString) tradesToSend += str + "»";

                dataToSend += tradesToSend;

                client.PlayerData.TradeString.Clear();

                return dataToSend;
            }
        }

        public static bool CompareModsWithClient(PlayerClient client, string data)
        {
            if (client.PlayerData.IsAdmin) return true;
            if (!StaticProxy.serverConfig.ModsSystem.MatchModlist) return true;

            string[] clientMods = data.Split('»');

            string flaggedMods = "";

            bool flagged = false;

            foreach (string clientMod in clientMods)
            {
                if (StaticProxy.modHandler.IsModWhitelisted(clientMod))
                    continue;
                else if (StaticProxy.modHandler.IsModBlacklisted(clientMod))
                {
                    flagged = true;
                    flaggedMods += clientMod + "»";
                }
                else if (!StaticProxy.modHandler.IsModEnforced(clientMod))
                {
                    flagged = true;
                    flaggedMods += clientMod + "»";
                }
            }

            foreach (var modMetaData in StaticProxy.modHandler.RequiredMods)
            {
                if (!clientMods.Contains(modMetaData.Name))
                {
                    flagged = true;
                    flaggedMods += modMetaData.Name + "»";
                }
            }

            if (flagged)
            {
                ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] " + "Doesn't Have The Required Mod Or Mod Files Mismatch!");
                flaggedMods = flaggedMods.Remove(flaggedMods.Count() - 1, 1);
                Networking.SendData(client, "Disconnect│WrongMods│" + flaggedMods);

                client.IsDisconnecting = true;
                return false;
            }
            else return true;
        }

        public static bool CompareConnectingClientWithConnecteds(PlayerClient client)
        {
            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient sc in clients)
            {
                if (sc.PlayerData.Username == client.PlayerData.Username)
                {
                    if (sc == client) continue;

                    Networking.SendData(sc, "Disconnect│AnotherLogin");
                    sc.IsDisconnecting = true;
                    return false;
                }
            }

            return true;
        }

        public static bool CompareConnectingClientVersion(PlayerClient client, string clientVersion)
        {
            if (string.IsNullOrWhiteSpace(Server.latestClientVersion)) return true;

            if (clientVersion == Server.latestClientVersion) return true;
            else
            {
                Networking.SendData(client, "Disconnect│Version");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] Tried To Join But Is Using Other Version");
                return false;
            }
        }

        public static bool CompareClientIPWithBans(PlayerClient client)
        {
            var banInfo = StaticProxy.playerHandler.GetBanInfo(client.PlayerData.Username);
            if (banInfo != null &&
                (banInfo.IPAddress == client.IPAddress.ToString() || banInfo.Username == client.PlayerData.Username))
            {
                Networking.SendData(client, "Disconnect│Banned");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] Tried To Join But Is Banned");
                return false;
            }

            return true;
        }

        public static bool CompareConnectingClientWithPlayerCount(PlayerClient client)
        {
            if (client.PlayerData.IsAdmin) return true;

            if (Networking.connectedClients.Count() >= StaticProxy.serverConfig.MaxPlayers + 1)
            {
                Networking.SendData(client, "Disconnect│ServerFull");
                client.IsDisconnecting = true;
                return false;
            }

            return true;
        }

        public static bool ParseClientUsername(PlayerClient client)
        {
            if (string.IsNullOrWhiteSpace(client.PlayerData.Username))
            {
                Networking.SendData(client, "Disconnect│Corrupted");
                client.IsDisconnecting = true;
                return false;
            }

            if (!client.PlayerData.Username.All(character => Char.IsLetterOrDigit(character) || character == '_' || character == '-'))
            {
                Networking.SendData(client, "Disconnect│Corrupted");
                client.IsDisconnecting = true;
                return false;
            }

            else return true;
        }
    }
}
