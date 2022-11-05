using System;
using System.Linq;
using OpenWorld.Shared.Networking.Packets;
using OpenWorldServer.Enums;
using OpenWorldServer.Handlers.Old;

namespace OpenWorldServer
{
    public static class JoiningsUtils
    {
        internal static void LoginProcedures(PlayerClient client, ConnectPacket packet)
        {
            client.Account.Username = packet.Username;
            client.Account.Password = packet.Password;


            if (!CompareConnectingClientWithPlayerCount(client))
                return;

            if (!CompareConnectingClientVersion(client, packet.Version))
                return;

            if (!StaticProxy.playerHandler.WhitelistHandler.IsWhitelisted(client))
            {
                Networking.SendData(client, "Disconnect│Whitelist");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] tried to Join but is not Whitelisted");
            }

            if (!CompareModsWithClient(client, packet.Mods))
                return;

            if (!CompareClientIPWithBans(client))
                return;

            if (!ParseClientUsername(client))
                return;

            CompareConnectingClientWithConnecteds(client);


            var playerData = StaticProxy.playerHandler.AccountsHandler.GetAccount(client);
            if (playerData == null)
            {
                StaticProxy.playerHandler.AccountsHandler.SaveAccount(client);
                ConsoleUtils.LogToConsole("New Player [" + client.Account.Username + "]");
            }
            else if (playerData.Password != client.Account.Password)
            {
                Networking.SendData(client, "Disconnect│WrongPassword");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] has been Kicked for: [Wrong Password]");
                return;
            }

            ConsoleUtils.UpdateTitle();
            ServerUtils.SendPlayerListToAll(client);

            CheckForJoinMode(client, packet.JoinMode);
        }

        private static void CheckForJoinMode(PlayerClient client, JoinMode joinMode)
        {
            if (joinMode == JoinMode.NewGame)
            {
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] has started a new game");
                SendNewGameData(client);
            }
            else if (joinMode == JoinMode.LoadGame)
            {
                PlayerUtils.GiveSavedDataToPlayer(client);
                SendLoadGameData(client);
            }

            ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] has Connected");
        }

        private static void SendNewGameData(PlayerClient client)
        {
            //We give saved data back to return data that is not removed at new creation
            PlayerUtils.GiveSavedDataToPlayer(client);
            StaticProxy.worldMapHandler.NotifySettlementRemoved(client.Account.HomeTileId);
            StaticProxy.playerHandler.AccountsHandler.ResetAccount(client, true);

            Networking.SendData(client, GetPlanetToSend());

            Networking.SendData(client, GetSettlementsToSend(client));

            Networking.SendData(client, GetVariablesToSend(client));

            var usernames = StaticProxy.playerHandler.ConnectedClients.Select(c => c.Account?.Username);
            client.SendData(new PlayerListPacket(usernames.ToArray()));

            Networking.SendData(client, FactionHandler.GetFactionDetails(client));

            Networking.SendData(client, FactionBuildingHandler.GetAllFactionStructures(client));

            Networking.SendData(client, "NewGame│");
        }

        private static void SendLoadGameData(PlayerClient client)
        {
            Networking.SendData(client, GetSettlementsToSend(client));

            Networking.SendData(client, GetVariablesToSend(client));

            var usernames = StaticProxy.playerHandler.ConnectedClients.Select(c => c.Account?.Username);
            client.SendData(new PlayerListPacket(usernames.ToArray()));

            Networking.SendData(client, FactionHandler.GetFactionDetails(client));

            Networking.SendData(client, FactionBuildingHandler.GetAllFactionStructures(client));

            Networking.SendData(client, GetGiftsToSend(client));

            Networking.SendData(client, GetTradesToSend(client));

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

            var settlementAccounts = StaticProxy.worldMapHandler.GetAccountsWithSettlements;
            if (settlementAccounts.Count == 0)
            {
                return dataToSend;
            }
            else
            {
                foreach (var settlementAccount in settlementAccounts)
                {
                    if (settlementAccount == client.Account)
                    {
                        continue;
                    }

                    int factionValue = 0;
                    if (client.Account.Faction == null)
                        factionValue = 0;
                    if (settlementAccount.Faction == null)
                        factionValue = 0;
                    else if (client.Account.Faction != null && settlementAccount.Faction != null)
                    {
                        if (client.Account.Faction.name == settlementAccount.Faction.name)
                        {
                            factionValue = 1;
                        }
                        else
                        {
                            factionValue = 2;
                        }
                    }

                    dataToSend += settlementAccount.HomeTileId + ":" + settlementAccount.Username + ":" + factionValue + "│";
                }

                return dataToSend;
            }
        }

        public static string GetVariablesToSend(PlayerClient client)
        {
            string dataToSend = "Variables│";

            if (Server.savedClients.Find(fetch => fetch.Account.Username == client.Account.Username) != null)
            {
                client.Account.IsAdmin = Server.savedClients.Find(fetch => fetch.Account.Username == client.Account.Username).Account.IsAdmin;
            }
            else client.Account.IsAdmin = false;

            int devInt = client.Account.IsAdmin || StaticProxy.serverConfig.AllowDevMode ? 1 : 0;

            int wipeInt = client.Account.ToWipe ? 1 : 0;

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

            if (client.Account.GiftString.Count == 0) return dataToSend;

            else
            {
                string giftsToSend = "";

                foreach (string str in client.Account.GiftString) giftsToSend += str + "»";

                dataToSend += giftsToSend;

                client.Account.GiftString.Clear();

                return dataToSend;
            }
        }

        public static string GetTradesToSend(PlayerClient client)
        {
            string dataToSend = "TradedItems│";

            if (client.Account.TradeString.Count == 0) return dataToSend;

            else
            {
                string tradesToSend = "";

                foreach (string str in client.Account.TradeString) tradesToSend += str + "»";

                dataToSend += tradesToSend;

                client.Account.TradeString.Clear();

                return dataToSend;
            }
        }

        public static bool CompareModsWithClient(PlayerClient client, string[] mods)
        {
            if (client.Account.IsAdmin) return true;
            if (!StaticProxy.serverConfig.ModsSystem.MatchModlist) return true;

            string[] clientMods = mods;

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
                flaggedMods = flaggedMods.Remove(flaggedMods.Count() - 1, 1);

                Networking.SendData(client, "Disconnect│WrongMods│" + flaggedMods);

                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] " + "Doesn't Have The Required Mod Or Mod Files Mismatch!");
                return false;
            }
            else return true;
        }

        public static void CompareConnectingClientWithConnecteds(PlayerClient client)
        {
            PlayerClient[] clients = StaticProxy.playerHandler.ConnectedClients.ToArray();
            foreach (PlayerClient sc in clients)
            {
                if (sc.Account.Username == client.Account.Username)
                {
                    if (sc == client) continue;

                    Networking.SendData(sc, "Disconnect│AnotherLogin");
                    sc.IsDisconnecting = true;
                    return;
                }
            }
        }

        public static bool CompareConnectingClientVersion(PlayerClient client, string clientVersion)
        {
            if (string.IsNullOrWhiteSpace(Server.latestClientVersion)) return true;

            if (clientVersion == Server.latestClientVersion) return true;
            else
            {
                Networking.SendData(client, "Disconnect│Version");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] Tried To Join But Is Using Other Version");
                return false;
            }
        }

        public static bool CompareClientIPWithBans(PlayerClient client)
        {
            var banInfo = StaticProxy.playerHandler.BanlistHandler.GetBanInfo(client.Account.Username);
            if (banInfo != null &&
                (banInfo.IPAddress == client.IPAddress.ToString() || banInfo.Username == client.Account.Username))
            {
                Networking.SendData(client, "Disconnect│Banned");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] Tried To Join But Is Banned");
                return false;
            }

            return true;
        }

        public static bool CompareConnectingClientWithPlayerCount(PlayerClient client)
        {
            if (client.Account.IsAdmin) return true;

            if (StaticProxy.playerHandler.ConnectedClients.Count >= StaticProxy.serverConfig.MaxPlayers + 1)
            {
                Networking.SendData(client, "Disconnect│ServerFull");
                client.IsDisconnecting = true;
                return false;
            }

            return true;
        }

        public static bool ParseClientUsername(PlayerClient client)
        {
            if (string.IsNullOrWhiteSpace(client.Account.Username))
            {
                Networking.SendData(client, "Disconnect│Corrupted");
                client.IsDisconnecting = true;
                return false;
            }

            if (!client.Account.Username.All(character => Char.IsLetterOrDigit(character) || character == '_' || character == '-'))
            {
                Networking.SendData(client, "Disconnect│Corrupted");
                client.IsDisconnecting = true;
                return false;
            }

            else return true;
        }
    }
}
