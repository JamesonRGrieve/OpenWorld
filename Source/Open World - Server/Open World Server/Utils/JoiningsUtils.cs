namespace OpenWorldServer
{
    public static class JoiningsUtils
    {

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
                    if (settlementAccount.Username == client.Account.Username)
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
    }
}