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
                client.PlayerData.HomeTileId = tileID;

                PlayerClient[] savedClients = Server.savedClients.ToArray();
                foreach (PlayerClient sc in savedClients)
                {
                    if (sc.PlayerData.Username == client.PlayerData.Username)
                    {
                        sc.PlayerData.HomeTileId = client.PlayerData.HomeTileId;
                        break;
                    }
                }

                StaticProxy.playerHandler.SavePlayerData(client);
            }

            int factionValue = 0;
            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient sc in clients)
            {
                if (sc.PlayerData.Username == client.PlayerData.Username) continue;
                else
                {
                    if (client.PlayerData.Faction == null) factionValue = 0;
                    if (sc.PlayerData.Faction == null) factionValue = 0;
                    else if (client.PlayerData.Faction != null && sc.PlayerData.Faction != null)
                    {
                        if (client.PlayerData.Faction.name == sc.PlayerData.Faction.name) factionValue = 1;
                        else factionValue = 2;
                    }
                }

                string dataString = "SettlementBuilder│AddSettlement│" + tileID + "│" + username + "│" + factionValue;
                Networking.SendData(sc, dataString);
            }

            Server.savedSettlements.Add(client.PlayerData.HomeTileId, new List<string> { client.PlayerData.Username });

            ConsoleUtils.LogToConsole("Settlement With ID [" + tileID + "] And Owner [" + username + "] Has Been Added");
        }

        public static void RemoveSettlement(PlayerClient? client, string tile)
        {
            if (client != null)
            {
                client.PlayerData.HomeTileId = null;

                PlayerClient[] savedClients = Server.savedClients.ToArray();
                foreach (PlayerClient sc in savedClients)
                {
                    if (sc.PlayerData.Username == client.PlayerData.Username)
                    {
                        sc.PlayerData.HomeTileId = null;
                        break;
                    }
                }

                StaticProxy.playerHandler.SavePlayerData(client);
            }

            if (!string.IsNullOrWhiteSpace(tile))
            {
                string dataString = "SettlementBuilder│RemoveSettlement│" + tile;

                PlayerClient[] clients = Networking.connectedClients.ToArray();
                foreach (PlayerClient sc in clients)
                {
                    if (client != null)
                    {
                        if (sc.PlayerData.Username == client.PlayerData.Username) continue;
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
                if (savedClient.PlayerData.Username == client.PlayerData.Username)
                {
                    if (savedClient.PlayerData.HomeTileId == tileID) return;

                    else
                    {
                        Dictionary<string, List<string>> settlements = Server.savedSettlements;
                        foreach (KeyValuePair<string, List<string>> pair in settlements)
                        {
                            if (pair.Value[0] == client.PlayerData.Username)
                            {
                                RemoveSettlement(client, pair.Key);
                                Thread.Sleep(500);
                                break;
                            }
                        }

                        break;
                    }
                }

                else
                {
                    if (savedClient.PlayerData.HomeTileId == tileID)
                    {
                        Networking.SendData(client, "Disconnect│Corrupted");

                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] Tried To Claim Used Tile! [" + tileID + "]");
                        Console.ForegroundColor = ConsoleColor.White;
                        return;
                    }
                }
            }

            AddSettlement(client, tileID, client.PlayerData.Username);
        }
    }
}
