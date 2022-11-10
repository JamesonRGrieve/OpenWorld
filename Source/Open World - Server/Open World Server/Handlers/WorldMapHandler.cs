using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenWorld.Shared.Data;
using OpenWorld.Shared.Networking.Packets;
using OpenWorldServer.Data;

namespace OpenWorldServer.Handlers
{
    public class WorldMapHandler
    {
        //private readonly AccountsHandler accountsHandler;
        private readonly PlayerHandler playerHandler;

        public IReadOnlyCollection<PlayerData> GetAccountsWithSettlements => this.playerHandler.AccountsHandler.Accounts.Where(a => a.HasSettlement).ToList();

        public IReadOnlyCollection<SettlementInfo> GetSettlements
            => this.playerHandler.AccountsHandler.Accounts
            .Where(a => a.HasSettlement)
            .Select(a => new SettlementInfo(a.HomeTileId, a.Username, a.Faction?.name)).ToList();

        public WorldMapHandler(PlayerHandler playerHandler)
        {
            this.playerHandler = playerHandler;
        }

        public bool IsTileAvailable(string tileID) => this.GetAccountFromTile(tileID) != null;

        public PlayerData GetAccountFromTile(string tileID) => this.GetAccountsWithSettlements.FirstOrDefault(a => a.HomeTileId == tileID);

        public void TryToClaimTile(PlayerClient client, string tileID)
        {
            var playerDataFromTile = StaticProxy.worldMapHandler.GetAccountFromTile(tileID);
            if (playerDataFromTile != null &&
                playerDataFromTile.Username != client.Account.Username)
            {
                Networking.SendData(client, "Disconnect│Corrupted");
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] tried to claim used Tile! [" + tileID + "]", ConsoleUtils.ConsoleLogMode.Error);
                return;
            }

            this.AddSettlement(client, tileID);
        }

        private void AddSettlement(PlayerClient client, string tileId)
        {
            client.Account.HomeTileId = tileId;
            StaticProxy.playerHandler.AccountsHandler.SaveAccount(client);

            int factionValue = 0;
            foreach (var connectedClient in this.playerHandler.ConnectedClients)
            {
                if (connectedClient.Account.Username == connectedClient.Account.Username)
                {
                    continue;
                }

                if (client.Account.Faction == null ||
                    connectedClient.Account.Faction == null)
                {
                    factionValue = 0;
                }
                else if (client.Account.Faction != null && connectedClient.Account.Faction != null)
                {
                    if (client.Account.Faction.name == connectedClient.Account.Faction.name)
                    {
                        factionValue = 1;
                    }
                    else
                    {
                        factionValue = 2;
                    }
                }

                var packet = new SettlementBuilderPacket(tileId, client.Account.Username, factionValue);
                connectedClient.SendData(packet);
            }

            ConsoleUtils.LogToConsole("Settlement with ID [" + tileId + "] and Owner [" + client.Account.Username + "] has been Added");
        }

        public void NotifySettlementAdded(PlayerClient client, int factionValue)
            => this.NotifySettlementAdded(client.Account?.HomeTileId, client.Account?.Username, factionValue);

        public void NotifySettlementAdded(string tileId, string username, int factionValue, PlayerClient executer = null)
        {
            if (string.IsNullOrEmpty(tileId))
            {
                return;
            }

            ConsoleUtils.LogToConsole($"Notifying addition of Settlement TileId [{username}]@[{tileId}]");
            this.NotifySettlementChange(new SettlementBuilderPacket(tileId, username, factionValue), executer);
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
        {
            Parallel.ForEach(this.playerHandler.ConnectedClients, target =>
            {
                if (executer?.Account?.Username != target.Account?.Username)
                {
                    target.SendData(packet);
                }
            });
        }
    }
}
