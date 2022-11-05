using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenWorld.Shared.Networking.Packets;
using OpenWorldServer.Data;

namespace OpenWorldServer.Handlers
{
    public class WorldMapHandler
    {
        //private readonly AccountsHandler accountsHandler;
        private readonly PlayerHandler playerHandler;

        public IReadOnlyCollection<PlayerData> GetAccountsWithSettlements => this.playerHandler.AccountsHandler.Accounts.Where(a => a.HasSettlement).ToList();

        public WorldMapHandler(PlayerHandler playerHandler)
        {
            this.playerHandler = playerHandler;
        }

        public bool IsTileAvailable(string tileID) => this.GetAccountFromTile(tileID) != null;

        public PlayerData GetAccountFromTile(string tileID) => this.GetAccountsWithSettlements.FirstOrDefault(a => a.HomeTileId == tileID);

        public void NotifySettlementAdded(string tileId, string username, int factionValue)
        {
            if (string.IsNullOrEmpty(tileId))
            {
                return;
            }

            ConsoleUtils.LogToConsole($"Notifying addition of Settlement TileId [{username}]@[{tileId}]");
            this.NotifySettlementChange(new SettlementBuilderPacket(tileId, username, factionValue));
        }

        public void NotifySettlementRemoved(string tileId)
        {
            if (string.IsNullOrEmpty(tileId))
            {
                return;
            }

            ConsoleUtils.LogToConsole($"Notifying removal of Settlement TileId [{tileId}]");
            this.NotifySettlementChange(new SettlementBuilderPacket(tileId));
        }

        private void NotifySettlementChange(SettlementBuilderPacket packet)
            => Parallel.ForEach(this.playerHandler.ConnectedClients.ToArray(), client => client.SendData(packet));
    }
}
