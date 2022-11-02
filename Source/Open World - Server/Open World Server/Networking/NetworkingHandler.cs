using OpenWorld.Shared.Networking.Packets;

namespace OpenWorldServer
{
    public static class NetworkingHandler
    {
        internal static void ConnectHandle(PlayerClient client, ConnectPacket packet)
        {
            JoiningsUtils.LoginProcedures(client, packet);
        }

        public static void ChatMessageHandle(PlayerClient client, string data)
        {
            ServerUtils.SendChatMessage(client, data);
        }

        public static void UserSettlementHandle(PlayerClient client, string data)
        {
            if (data.StartsWith("UserSettlement│NewSettlementID│"))
            {
                try
                {
                    client.PlayerData.Wealth = float.Parse(data.Split('│')[3]);
                    client.PlayerData.PawnCount = int.Parse(data.Split('│')[4]);

                    PlayerUtils.CheckForPlayerWealth(client);
                }
                catch { }

                WorldUtils.CheckForTileDisponibility(client, data.Split('│')[2]);
            }

            else if (data.StartsWith("UserSettlement│AbandonSettlementID│"))
            {
                if (client.PlayerData.HomeTileId != data.Split('│')[2] || string.IsNullOrWhiteSpace(client.PlayerData.HomeTileId)) return;
                else WorldUtils.RemoveSettlement(client, data.Split('│')[2]);
            }

            else if (data == "UserSettlement│NoSettlementInLoad")
            {
                if (string.IsNullOrWhiteSpace(client.PlayerData.HomeTileId)) return;
                else WorldUtils.RemoveSettlement(client, client.PlayerData.HomeTileId);
            }
        }

        public static void ForceEventHandle(PlayerClient client, string data)
        {
            string dataToSend = "";

            if (PlayerUtils.CheckForConnectedPlayers(data.Split('│')[2]))
            {
                if (PlayerUtils.CheckForPlayerShield(data.Split('│')[2]))
                {
                    dataToSend = "│SentEvent│Confirm│";

                    PlayerUtils.SendEventToPlayer(client, data);
                }

                else dataToSend = "│SentEvent│Deny│";
            }
            else dataToSend = "│SentEvent│Deny│";

            Networking.SendData(client, dataToSend);
        }

        public static void SendGiftHandle(PlayerClient client, string data)
        {
            PlayerUtils.SendGiftToPlayer(client, data);
        }

        public static void SendTradeHandle(PlayerClient client, string data)
        {
            string dataToSend = "";

            if (PlayerUtils.CheckForConnectedPlayers(data.Split('│')[1]))
            {
                dataToSend = "│SentTrade│Confirm│";

                PlayerUtils.SendTradeRequestToPlayer(client, data);
            }
            else dataToSend = "│SentTrade│Deny│";

            Networking.SendData(client, dataToSend);
        }

        public static void SendBarterHandle(PlayerClient client, string data)
        {
            string dataToSend = "";

            if (PlayerUtils.CheckForConnectedPlayers(data.Split('│')[1]))
            {
                dataToSend = "│SentBarter│Confirm│";

                PlayerUtils.SendBarterRequestToPlayer(client, data);
            }
            else dataToSend = "│SentBarter│Deny│";

            Networking.SendData(client, dataToSend);
        }

        public static void TradeStatusHandle(PlayerClient client, string data)
        {
            string username = data.Split('│')[2];
            PlayerClient target = null;

            foreach (PlayerClient sc in Networking.connectedClients)
            {
                if (sc.PlayerData.Username == username)
                {
                    target = sc;
                    break;
                }
            }

            if (target == null) return;

            if (data.StartsWith("TradeStatus│Deal│"))
            {
                Networking.SendData(target, "│SentTrade│Deal│");

                ConsoleUtils.LogToConsole("Trade Done Between [" + target.PlayerData.Username + "] And [" + client.PlayerData.Username + "]");
            }

            else if (data.StartsWith("TradeStatus│Reject│"))
            {
                Networking.SendData(target, "│SentTrade│Reject│");
            }
        }

        public static void BarterStatusHandle(PlayerClient client, string data)
        {
            string user = data.Split('│')[2];
            PlayerClient target = null;

            foreach (PlayerClient sc in Networking.connectedClients)
            {
                if (sc.PlayerData.Username == user)
                {
                    target = sc;
                    break;
                }

                if (sc.PlayerData.HomeTileId == user)
                {
                    target = sc;
                    break;
                }
            }

            if (target == null) return;

            if (data.StartsWith("BarterStatus│Deal│"))
            {
                Networking.SendData(target, "│SentBarter│Deal│");

                ConsoleUtils.LogToConsole("Barter Done Between [" + target.PlayerData.Username + "] And [" + client.PlayerData.Username + "]");
            }

            else if (data.StartsWith("BarterStatus│Reject│"))
            {
                Networking.SendData(target, "│SentBarter│Reject│");
            }

            else if (data.StartsWith("BarterStatus│Rebarter│"))
            {
                Networking.SendData(target, "│SentBarter│Rebarter│" + client.PlayerData.Username + "│" + data.Split('│')[3]);
            }
        }

        public static void SpyInfoHandle(PlayerClient client, string data)
        {
            string dataToSend = "";

            if (PlayerUtils.CheckForConnectedPlayers(data.Split('│')[1]))
            {
                dataToSend = "│SentSpy│Confirm│" + PlayerUtils.GetSpyData(data.Split('│')[1], client);
            }
            else dataToSend = "│SentSpy│Deny│";

            Networking.SendData(client, dataToSend);
        }

        public static void FactionManagementHandle(PlayerClient client, string data)
        {
            if (data == "FactionManagement│Refresh")
            {
                if (client.PlayerData.Faction == null) return;
                else Networking.SendData(client, FactionHandler.GetFactionDetails(client));
            }

            else if (data.StartsWith("FactionManagement│Create│"))
            {
                if (client.PlayerData.Faction != null) return;

                string factionName = data.Split('│')[2];

                if (string.IsNullOrWhiteSpace(factionName)) return;

                Faction factionToFetch = Server.savedFactions.Find(fetch => fetch.name == factionName);
                if (factionToFetch == null) FactionHandler.CreateFaction(factionName, client);
                else Networking.SendData(client, "FactionManagement│NameInUse");
            }

            else if (data == "FactionManagement│Disband")
            {
                if (client.PlayerData.Faction == null) return;

                if (FactionHandler.GetMemberPowers(client.PlayerData.Faction, client) != FactionHandler.MemberRank.Leader)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                Faction factionToCheck = client.PlayerData.Faction;
                FactionHandler.PurgeFaction(factionToCheck);
            }

            else if (data == "FactionManagement│Leave")
            {
                if (client.PlayerData.Faction == null) return;

                FactionHandler.RemoveMember(client.PlayerData.Faction, client);
            }

            else if (data.StartsWith("FactionManagement│Join│"))
            {
                string factionString = data.Split('│')[2];

                Faction factionToJoin = Server.savedFactions.Find(fetch => fetch.name == factionString);

                if (factionToJoin == null) return;
                else FactionHandler.AddMember(factionToJoin, client);
            }

            else if (data.StartsWith("FactionManagement│AddMember"))
            {
                if (client.PlayerData.Faction == null) return;

                if (FactionHandler.GetMemberPowers(client.PlayerData.Faction, client) == FactionHandler.MemberRank.Member)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                string tileID = data.Split('│')[2];

                if (string.IsNullOrWhiteSpace(tileID)) return;

                if (!PlayerUtils.CheckForConnectedPlayers(tileID)) Networking.SendData(client, "PlayerNotConnected│");
                else
                {
                    PlayerClient memberToAdd = PlayerUtils.GetPlayerFromTile(tileID);
                    if (memberToAdd.PlayerData.Faction != null) Networking.SendData(client, "FactionManagement│AlreadyInFaction");
                    else Networking.SendData(memberToAdd, "FactionManagement│Invite│" + client.PlayerData.Faction.name);
                }
            }

            else if (data.StartsWith("FactionManagement│RemoveMember"))
            {
                if (client.PlayerData.Faction == null) return;

                if (FactionHandler.GetMemberPowers(client.PlayerData.Faction, client) == FactionHandler.MemberRank.Member)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                string tileID = data.Split('│')[2];

                if (string.IsNullOrWhiteSpace(tileID)) return;

                if (!PlayerUtils.CheckForConnectedPlayers(tileID))
                {
                    Faction factionToCheck = Server.savedFactions.Find(fetch => fetch.name == client.PlayerData.Faction.name);
                    PlayerClient memberToRemove = Server.savedClients.Find(fetch => fetch.PlayerData.HomeTileId == tileID);

                    if (memberToRemove.PlayerData.Faction == null) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else if (memberToRemove.PlayerData.Faction.name != factionToCheck.name) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else FactionHandler.RemoveMember(factionToCheck, memberToRemove);
                }

                else
                {
                    PlayerClient memberToRemove = PlayerUtils.GetPlayerFromTile(tileID);

                    if (memberToRemove.PlayerData.Faction == null) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else if (memberToRemove.PlayerData.Faction != client.PlayerData.Faction) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else FactionHandler.RemoveMember(client.PlayerData.Faction, memberToRemove);
                }
            }

            else if (data.StartsWith("FactionManagement│PromoteMember"))
            {
                if (client.PlayerData.Faction == null) return;

                if (FactionHandler.GetMemberPowers(client.PlayerData.Faction, client) != FactionHandler.MemberRank.Leader)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                string tileID = data.Split('│')[2];

                if (string.IsNullOrWhiteSpace(tileID)) return;

                if (!PlayerUtils.CheckForConnectedPlayers(tileID))
                {
                    Faction factionToCheck = Server.savedFactions.Find(fetch => fetch.name == client.PlayerData.Faction.name);
                    PlayerClient memberToPromote = Server.savedClients.Find(fetch => fetch.PlayerData.HomeTileId == tileID);

                    if (memberToPromote.PlayerData.Faction == null) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else if (memberToPromote.PlayerData.Faction.name != factionToCheck.name) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else FactionHandler.PromoteMember(factionToCheck, memberToPromote);
                }

                else
                {
                    PlayerClient memberToPromote = PlayerUtils.GetPlayerFromTile(tileID);

                    if (memberToPromote.PlayerData.Faction == null) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else if (memberToPromote.PlayerData.Faction != client.PlayerData.Faction) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else FactionHandler.PromoteMember(client.PlayerData.Faction, memberToPromote);
                }
            }

            else if (data.StartsWith("FactionManagement│DemoteMember"))
            {
                if (client.PlayerData.Faction == null) return;

                if (FactionHandler.GetMemberPowers(client.PlayerData.Faction, client) != FactionHandler.MemberRank.Leader)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                string tileID = data.Split('│')[2];

                if (string.IsNullOrWhiteSpace(tileID)) return;

                if (!PlayerUtils.CheckForConnectedPlayers(tileID))
                {
                    Faction factionToCheck = Server.savedFactions.Find(fetch => fetch.name == client.PlayerData.Faction.name);
                    PlayerClient memberToDemote = Server.savedClients.Find(fetch => fetch.PlayerData.HomeTileId == tileID);

                    if (memberToDemote.PlayerData.Faction == null) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else if (memberToDemote.PlayerData.Faction.name != factionToCheck.name) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else FactionHandler.DemoteMember(factionToCheck, memberToDemote);
                }

                else
                {
                    PlayerClient memberToDemote = PlayerUtils.GetPlayerFromTile(tileID);

                    if (memberToDemote.PlayerData.Faction == null) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else if (memberToDemote.PlayerData.Faction != client.PlayerData.Faction) Networking.SendData(client, "FactionManagement│NotInFaction");
                    else FactionHandler.DemoteMember(client.PlayerData.Faction, memberToDemote);
                }
            }

            else if (data.StartsWith("FactionManagement│BuildStructure"))
            {
                if (client.PlayerData.Faction == null) return;

                if (FactionHandler.GetMemberPowers(client.PlayerData.Faction, client) != FactionHandler.MemberRank.Leader)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                string tileID = data.Split('│')[2];
                string structureID = data.Split('│')[3];

                if (string.IsNullOrWhiteSpace(tileID)) return;

                if (string.IsNullOrWhiteSpace(structureID)) return;

                FactionBuildingHandler.BuildStructure(client.PlayerData.Faction, tileID, structureID);
            }

            else if (data.StartsWith("FactionManagement│DestroyStructure"))
            {
                if (client.PlayerData.Faction == null) return;

                if (FactionHandler.GetMemberPowers(client.PlayerData.Faction, client) != FactionHandler.MemberRank.Leader)
                {
                    Networking.SendData(client, "FactionManagement│NoPowers");
                    return;
                }

                string tileID = data.Split('│')[2];

                if (string.IsNullOrWhiteSpace(tileID)) return;

                FactionBuildingHandler.DestroyStructure(client.PlayerData.Faction, tileID);
            }

            else if (data.StartsWith("FactionManagement│Silo"))
            {
                if (client.PlayerData.Faction == null) return;

                string siloID = data.Split('│')[3];

                if (data.StartsWith("FactionManagement│Silo│Check"))
                {
                    if (string.IsNullOrWhiteSpace(siloID)) return;

                    Networking.SendData(client, FactionSiloHandler.GetSiloContents(client.PlayerData.Faction, siloID));
                }

                if (data.StartsWith("FactionManagement│Silo│Deposit"))
                {
                    string items = data.Split('│')[4];

                    if (string.IsNullOrWhiteSpace(siloID)) return;

                    if (string.IsNullOrWhiteSpace(items)) return;

                    FactionSiloHandler.DepositIntoSilo(client.PlayerData.Faction, siloID, items);
                }

                if (data.StartsWith("FactionManagement│Silo│Withdraw"))
                {
                    string itemID = data.Split('│')[4];

                    if (string.IsNullOrWhiteSpace(siloID)) return;

                    if (string.IsNullOrWhiteSpace(itemID)) return;

                    FactionSiloHandler.WithdrawFromSilo(client.PlayerData.Faction, siloID, itemID, client);
                }
            }
        }
    }
}