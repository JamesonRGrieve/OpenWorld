using System.Collections.Generic;
using System.Linq;
using OpenWorld.Shared.Data;
using OpenWorld.Shared.Enums;
using OpenWorld.Shared.Networking.Packets;
using OpenWorldServer.Data;
using OpenWorldServer.Manager;

namespace OpenWorldServer.Handlers
{
    public class WorldMapHandler
    {
        //private readonly AccountsHandler accountsHandler;
        private readonly PlayerManager playerManager;

        public IReadOnlyCollection<Account> GetAccountsWithSettlements => this.playerManager.AccountsHandler.Accounts.Where(a => a.HasSettlement).ToList();

        public IReadOnlyCollection<SettlementInfo> GetSettlements
            => this.playerManager.AccountsHandler.Accounts
            .Where(a => a.HasSettlement)
            .Select(a => new SettlementInfo(a.Id, a.Username, a.HomeTileId, a.Faction?.name)).ToList();

        public WorldMapHandler(PlayerManager playerHandler)
        {
            this.playerManager = playerHandler;
        }

        public bool IsTileAvailable(string tileID) => this.GetAccountFromTile(tileID) != null;

        public Account GetAccountFromTile(string tileID) => this.GetAccountsWithSettlements.FirstOrDefault(a => a.HomeTileId == tileID);

        public void TryToClaimTile(PlayerClient client, string tileID)
        {
            var accountFromTile = this.GetAccountFromTile(tileID);
            if (accountFromTile != null &&
                accountFromTile.Id != client.Account.Id)
            {
                client.Disconnect(OpenWorld.Shared.Enums.DisconnectReason.Corrupted);
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] tried to claim used Tile! [" + tileID + "]", ConsoleUtils.ConsoleLogMode.Error);
                return;
            }

            this.AddSettlement(client, tileID);
        }

        private void AddSettlement(PlayerClient client, string tileId)
        {
            client.Account.HomeTileId = tileId;
            this.playerManager.AccountsHandler.SaveAccount(client);

            foreach (var connectedClient in this.playerManager.ConnectedClients)
            {
                if (connectedClient.Account.Id == connectedClient.Account.Id)
                {
                    continue;
                }

                var factionType = SettlementFactionType.NoFaction;
                if (!string.IsNullOrEmpty(client.Account.Faction?.name) && client.Account.Faction.name == connectedClient.Account?.Faction?.name)
                {
                    factionType = SettlementFactionType.SameFaction;
                }
                if (!string.IsNullOrEmpty(connectedClient.Account?.Faction?.name))
                {
                    factionType = SettlementFactionType.OtherFaciton;
                }

                var packet = new SettlementBuilderPacket(tileId, client.Account.Username, factionType);
                connectedClient.SendData(packet);
            }

            ConsoleUtils.LogToConsole("Settlement with ID [" + tileId + "] and Owner [" + client.Account.Username + "] has been added");
        }

        public void NotifySettlementAdded(PlayerClient client, SettlementFactionType factionType)
            => this.NotifySettlementAdded(client.Account?.HomeTileId, client.Account?.Username, factionType);

        public void NotifySettlementAdded(string tileId, string username, SettlementFactionType factionType, PlayerClient executer = null)
        {
            if (string.IsNullOrEmpty(tileId))
            {
                return;
            }

            ConsoleUtils.LogToConsole($"Notifying addition of Settlement TileId [{username}]@[{tileId}]");
            this.NotifySettlementChange(new SettlementBuilderPacket(tileId, username, factionType), executer);
        }

        public void NotifySettlementRemoved(PlayerClient client)
            => this.NotifySettlementRemoved(client.Account?.HomeTileId, client);

        public void NotifySettlementRemoved(string tileId, PlayerClient executer = null)
        {
            if (string.IsNullOrEmpty(tileId))
            {
                return;
            }

            ConsoleUtils.LogToConsole($"Notifying removal of Settlement TileId [{tileId}]");
            this.NotifySettlementChange(new SettlementBuilderPacket(tileId), executer);
        }

        private void NotifySettlementChange(SettlementBuilderPacket packet, PlayerClient executer = null)
            => this.playerManager.SendPacketToAll(packet, executer);
    }
}
