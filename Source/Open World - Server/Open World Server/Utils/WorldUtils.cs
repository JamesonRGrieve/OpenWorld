using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenWorldServer
{
    public static class WorldUtils
    {
        public static void AddSettlement(PlayerClient? client, string tileID, string username)
        {
            if (client != null)
            {
                client.Account.HomeTileId = tileID;

                PlayerClient[] savedClients = Server.savedClients.ToArray();
                foreach (PlayerClient sc in savedClients)
                {
                    if (sc.Account.Username == client.Account.Username)
                    {
                        sc.Account.HomeTileId = client.Account.HomeTileId;
                        break;
                    }
                }

                StaticProxy.playerHandler.AccountsHandler.SaveAccount(client);
            }

            int factionValue = 0;
            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient sc in clients)
            {
                if (sc.Account.Username == client.Account.Username) continue;
                else
                {
                    if (client.Account.Faction == null) factionValue = 0;
                    if (sc.Account.Faction == null) factionValue = 0;
                    else if (client.Account.Faction != null && sc.Account.Faction != null)
                    {
                        if (client.Account.Faction.name == sc.Account.Faction.name) factionValue = 1;
                        else factionValue = 2;
                    }
                }

                string dataString = "SettlementBuilder│AddSettlement│" + tileID + "│" + username + "│" + factionValue;
                Networking.SendData(sc, dataString);
            }

            Server.savedSettlements.Add(client.Account.HomeTileId, new List<string> { client.Account.Username });

            ConsoleUtils.LogToConsole("Settlement With ID [" + tileID + "] And Owner [" + username + "] Has Been Added");
        }

        public static void RemoveSettlement(PlayerClient? client, string tile)
        {
            if (client != null)
            {
                client.Account.HomeTileId = null;

                PlayerClient[] savedClients = Server.savedClients.ToArray();
                foreach (PlayerClient sc in savedClients)
                {
                    if (sc.Account.Username == client.Account.Username)
                    {
                        sc.Account.HomeTileId = null;
                        break;
                    }
                }

                StaticProxy.playerHandler.AccountsHandler.SaveAccount(client);
            }

            if (!string.IsNullOrWhiteSpace(tile))
            {
                string dataString = "SettlementBuilder│RemoveSettlement│" + tile;

                PlayerClient[] clients = Networking.connectedClients.ToArray();
                foreach (PlayerClient sc in clients)
                {
                    if (client != null)
                    {
                        if (sc.Account.Username == client.Account.Username) continue;
                    }

                    Networking.SendData(sc, dataString);
                }

                Server.savedSettlements.Remove(tile);

                ConsoleUtils.LogToConsole("Settlement With ID [" + tile + "] Has Been Deleted");
            }
        }

        public static void CheckForTileDisponibility(PlayerClient client, string tileID)
        {
            PlayerClient[] savedClients = Server.savedClients.ToArray();
            foreach (PlayerClient savedClient in savedClients)
            {
                if (savedClient.Account.Username == client.Account.Username)
                {
                    if (savedClient.Account.HomeTileId == tileID) return;

                    else
                    {
                        Dictionary<string, List<string>> settlements = Server.savedSettlements;
                        foreach (KeyValuePair<string, List<string>> pair in settlements)
                        {
                            if (pair.Value[0] == client.Account.Username)
                            {
                                RemoveSettlement(client, pair.Key);
                                break;
                            }
                        }

                        break;
                    }
                }

                else
                {
                    if (savedClient.Account.HomeTileId == tileID)
                    {
                        Networking.SendData(client, "Disconnect│Corrupted");

                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] Tried To Claim Used Tile! [" + tileID + "]");
                        Console.ForegroundColor = ConsoleColor.White;
                        return;
                    }
                }
            }

            AddSettlement(client, tileID, client.Account.Username);
        }
    }
}
