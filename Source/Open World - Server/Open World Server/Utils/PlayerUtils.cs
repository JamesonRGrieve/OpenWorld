using System;
using System.Collections.Generic;
using System.IO;
using OpenWorldServer.Services;

namespace OpenWorldServer
{
    public static class PlayerUtils
    {
        public static void LoadPlayer(string path)
        {
            try
            {
                //BinaryFormatter formatter = new BinaryFormatter();
                //FileStream s = File.Open(path, FileMode.Open);
                //object obj = formatter.Deserialize(s);
                //ServerClient playerToLoad = (ServerClient)obj;

                //s.Flush();
                //s.Close();
                //s.Dispose();



                var playerData = StaticProxy.playerHandler.GetPlayerData(Path.GetFileNameWithoutExtension(path));
                if (playerData == null)
                    return;

                PlayerClient playerToLoad = new PlayerClient(null) { PlayerData = playerData };

                if (!string.IsNullOrWhiteSpace(playerToLoad.PlayerData.HomeTileId))
                {
                    try { Server.savedSettlements.Add(playerToLoad.PlayerData.HomeTileId, new List<string>() { playerToLoad.PlayerData.Username }); }
                    catch
                    {
                        playerToLoad.PlayerData.HomeTileId = null;
                        StaticProxy.playerHandler.SavePlayerData(playerToLoad.PlayerData);

                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Error! Player " + playerToLoad.PlayerData.Username + " Is Using A Cloned Entry! Fixing");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }

                if (playerToLoad.PlayerData.Faction != null)
                {
                    Faction factionToFech = Server.savedFactions.Find(fetch => fetch.name == playerToLoad.PlayerData.Faction.name);
                    if (factionToFech == null)
                    {
                        playerToLoad.PlayerData.Faction = null;
                        StaticProxy.playerHandler.SavePlayerData(playerToLoad.PlayerData);

                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Error! Player " + playerToLoad.PlayerData.Username + " Is Using A Missing Faction! Fixing");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }

                Server.savedClients.Add(playerToLoad);
            }

            catch { }
        }

        public static void GiveSavedDataToPlayer(PlayerClient client)
        {
            var playerData = StaticProxy.playerHandler.GetPlayerData(client);
            if (playerData != null)
            {
                return;
            }

            client.PlayerData = playerData;
            if (client.PlayerData.Faction != null)
            {
                Faction factionToGive = Server.savedFactions.Find(fetch => fetch.name == client.PlayerData.Faction.name);
                if (factionToGive != null) client.PlayerData.Faction = factionToGive;
                else client.PlayerData.Faction = null;
            }
        }

        public static void CheckAllAvailablePlayers(bool newLine)
        {
            if (newLine) Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Players Check:");
            Console.ForegroundColor = ConsoleColor.White;

            CheckSavedPlayers();
            Console.WriteLine("");
        }

        private static void CheckSavedPlayers()
        {
            //Server.savedClients.Clear();
            //Server.savedSettlements.Clear();

            if (!Directory.Exists(PathProvider.PlayersFolderPath))
            {
                Directory.CreateDirectory(PathProvider.PlayersFolderPath);
                ConsoleUtils.LogToConsole("No Players Folder Found, Generating");
                return;
            }

            else
            {
                string[] playerFiles = Directory.GetFiles(PathProvider.PlayersFolderPath);

                foreach (string file in playerFiles)
                {
                    if (StaticProxy.serverConfig.IdleSystem.IsActive)
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.LastAccessTime < DateTime.Now.AddDays(-StaticProxy.serverConfig.IdleSystem.IdleThresholdInDays))
                        {
                            fi.Delete();
                        }
                    }

                    LoadPlayer(file);
                }

                if (Server.savedClients.Count == 0) ConsoleUtils.LogToConsole("No Saved Players Found, Ignoring");
                else ConsoleUtils.LogToConsole("Loaded [" + Server.savedClients.Count + "] Player Files");
            }
        }

        public static void CheckForPlayerWealth(PlayerClient client)
        {
            if (StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.IsActive == false) return;
            if (StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.BanThreshold == 0 && StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.WarningThreshold == 0) return;
            if (client.PlayerData.IsAdmin) return;

            int wealthToCompare = (int)Server.savedClients.Find(fetch => fetch.PlayerData.Username == client.PlayerData.Username).PlayerData.Wealth;

            if (client.PlayerData.Wealth - wealthToCompare > StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.BanThreshold && StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.BanThreshold > 0)
            {
                StaticProxy.playerHandler.SavePlayerData(client);
                Server.savedClients.Find(fetch => fetch.PlayerData.Username == client.PlayerData.Username).PlayerData.Wealth = client.PlayerData.Wealth;
                Server.savedClients.Find(fetch => fetch.PlayerData.Username == client.PlayerData.Username).PlayerData.PawnCount = client.PlayerData.PawnCount;

                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "]'s Wealth Triggered Alarm [" + wealthToCompare + " > " + (int)client.PlayerData.Wealth + "], Banning");
                Console.ForegroundColor = ConsoleColor.White;

                StaticProxy.playerHandler.BanPlayer(client, "Wealth Check triggered");
            }
            else if (client.PlayerData.Wealth - wealthToCompare > StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.WarningThreshold && StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.WarningThreshold > 0)
            {
                StaticProxy.playerHandler.SavePlayerData(client);
                Server.savedClients.Find(fetch => fetch.PlayerData.Username == client.PlayerData.Username).PlayerData.Wealth = client.PlayerData.Wealth;
                Server.savedClients.Find(fetch => fetch.PlayerData.Username == client.PlayerData.Username).PlayerData.PawnCount = client.PlayerData.PawnCount;

                Console.ForegroundColor = ConsoleColor.Yellow;
                ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "]'s Wealth Triggered Warning [" + wealthToCompare + " > " + (int)client.PlayerData.Wealth + "]");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                StaticProxy.playerHandler.SavePlayerData(client);
                Server.savedClients.Find(fetch => fetch.PlayerData.Username == client.PlayerData.Username).PlayerData.Wealth = client.PlayerData.Wealth;
                Server.savedClients.Find(fetch => fetch.PlayerData.Username == client.PlayerData.Username).PlayerData.PawnCount = client.PlayerData.PawnCount;
            }
        }

        public static bool CheckForConnectedPlayers(string tileID)
        {
            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient client in clients)
            {
                if (client.PlayerData.HomeTileId == tileID) return true;
            }

            return false;
        }

        public static PlayerClient GetPlayerFromTile(string tileID)
        {
            return Networking.connectedClients.Find(fetch => fetch.PlayerData.HomeTileId == tileID);
        }

        public static bool CheckForPlayerShield(string tileID)
        {
            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient client in clients)
            {
                if (client.PlayerData.HomeTileId == tileID && !client.IsEventProtected && !client.PlayerData.IsImmunized)
                {
                    client.IsEventProtected = true;
                    return true;
                }
            }

            return false;
        }

        public static bool CheckForPvpAvailability(string tileID)
        {
            foreach (PlayerClient client in Networking.connectedClients)
            {
                if (client.PlayerData.HomeTileId == tileID && !client.InRTSE && !client.PlayerData.IsImmunized)
                {
                    client.InRTSE = true;
                    return true;
                }
            }

            return false;
        }

        public static string GetSpyData(string tileID, PlayerClient origin)
        {
            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient client in clients)
            {
                if (client.PlayerData.HomeTileId == tileID)
                {
                    string dataToReturn = client.PlayerData.PawnCount.ToString() + "»" + client.PlayerData.Wealth.ToString() + "»" + client.IsEventProtected + "»" + client.InRTSE;

                    if (client.PlayerData.GiftString.Count > 0) dataToReturn += "»" + "True";
                    else dataToReturn += "»" + "False";

                    if (client.PlayerData.TradeString.Count > 0) dataToReturn += "»" + "True";
                    else dataToReturn += "»" + "False";

                    Random rnd = new Random();
                    int chance = rnd.Next(0, 2);
                    if (chance == 1) Networking.SendData(client, "Spy│" + origin.PlayerData.Username);

                    ConsoleUtils.LogToConsole("Spy Done Between [" + origin.PlayerData.Username + "] And [" + client.PlayerData.Username + "]");

                    return dataToReturn;
                }
            }

            return "";
        }

        public static void SendEventToPlayer(PlayerClient invoker, string data)
        {
            string dataToSend = "ForcedEvent│" + data.Split('│')[1];

            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient sc in clients)
            {
                if (sc.PlayerData.HomeTileId == data.Split('│')[2])
                {
                    ConsoleUtils.LogToConsole("Player [" + invoker.PlayerData.Username + "] Has Sent Forced Event [" + data.Split('│')[1] + "] To [" + sc.PlayerData.Username + "]");
                    Networking.SendData(sc, dataToSend);
                    break;
                }
            }
        }

        public static void SendGiftToPlayer(PlayerClient invoker, string data)
        {
            string tileToSend = data.Split('│')[1];
            string dataToSend = "GiftedItems│" + data.Split('│')[2];

            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient sc in clients)
            {
                if (sc.PlayerData.HomeTileId == tileToSend)
                {
                    Networking.SendData(sc, dataToSend);
                    ConsoleUtils.LogToConsole("Gift Done Between [" + invoker.PlayerData.Username + "] And [" + sc.PlayerData.Username + "]");
                    return;
                }
            }

            dataToSend = dataToSend.Replace("GiftedItems│", "");

            PlayerClient[] savedClients = Server.savedClients.ToArray();
            foreach (PlayerClient sc in savedClients)
            {
                if (sc.PlayerData.HomeTileId == tileToSend)
                {
                    sc.PlayerData.GiftString.Add(dataToSend);
                    StaticProxy.playerHandler.SavePlayerData(sc);
                    ConsoleUtils.LogToConsole("Gift Done Between [" + invoker.PlayerData.Username + "] And [" + sc.PlayerData.Username + "] But Was Offline. Saving");
                    return;
                }
            }
        }

        public static void SendTradeRequestToPlayer(PlayerClient invoker, string data)
        {
            string dataToSend = "TradeRequest│" + invoker.PlayerData.Username + "│" + data.Split('│')[2] + "│" + data.Split('│')[3];

            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient sc in clients)
            {
                if (sc.PlayerData.HomeTileId == data.Split('│')[1])
                {
                    Networking.SendData(sc, dataToSend);
                    return;
                }
            }
        }

        public static void SendBarterRequestToPlayer(PlayerClient invoker, string data)
        {
            string dataToSend = "BarterRequest│" + invoker.PlayerData.HomeTileId + "│" + data.Split('│')[2];

            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient sc in clients)
            {
                if (sc.PlayerData.HomeTileId == data.Split('│')[1])
                {
                    Networking.SendData(sc, dataToSend);
                    return;
                }
            }
        }
    }
}
