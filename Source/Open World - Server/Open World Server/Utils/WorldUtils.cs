using System;
using System.Linq;
using OpenWorld.Shared.Networking.Packets;

namespace OpenWorldServer
{
    public static class WorldUtils
    {
        public static void AddSettlement(PlayerClient? client, string tileId, string username)
        {
            client.Account.HomeTileId = tileId;
            StaticProxy.playerHandler.AccountsHandler.SaveAccount(client);

            int factionValue = 0;
            foreach (var connectedClient in StaticProxy.playerHandler.ConnectedClients.ToArray())
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

                var packet = new SettlementBuilderPacket(tileId, username, factionValue);
                connectedClient.SendData(packet);
                StaticProxy.worldMapHandler.NotifySettlementAdded(tileId, username, factionValue);
            }

            ConsoleUtils.LogToConsole("Settlement With ID [" + tileId + "] And Owner [" + username + "] Has Been Added");
        }

        public static void CheckForTileDisponibility(PlayerClient client, string tileID)
        {
            var playerDataFromTile = StaticProxy.worldMapHandler.GetAccountFromTile(tileID);
            if (playerDataFromTile != null &&
                playerDataFromTile.Username != client.Account.Username)
            {
                Networking.SendData(client, "Disconnect│Corrupted");
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] tried to claim used Tile! [" + tileID + "]", ConsoleColor.Red);
                return;
            }

            AddSettlement(client, tileID, client.Account.Username);
        }
    }
}
