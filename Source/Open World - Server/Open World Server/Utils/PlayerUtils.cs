using System;
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



                var playerData = StaticProxy.playerHandler.AccountsHandler.GetAccount(Path.GetFileNameWithoutExtension(path));
                if (playerData == null)
                    return;

                PlayerClient playerToLoad = new PlayerClient(null) { Account = playerData };

                if (!string.IsNullOrWhiteSpace(playerToLoad.Account.HomeTileId))
                {
                    try
                    {
                        //Server.savedSettlements.Add(playerToLoad.Account.HomeTileId, new List<string>() { playerToLoad.Account.Username });
                    }
                    catch
                    {
                        playerToLoad.Account.HomeTileId = null;
                        StaticProxy.playerHandler.AccountsHandler.SaveAccount(playerToLoad.Account);

                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Error! Player " + playerToLoad.Account.Username + " Is Using A Cloned Entry! Fixing");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }

                if (playerToLoad.Account.Faction != null)
                {
                    Faction factionToFech = Server.savedFactions.Find(fetch => fetch.name == playerToLoad.Account.Faction.name);
                    if (factionToFech == null)
                    {
                        playerToLoad.Account.Faction = null;
                        StaticProxy.playerHandler.AccountsHandler.SaveAccount(playerToLoad.Account);

                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Error! Player " + playerToLoad.Account.Username + " Is Using A Missing Faction! Fixing");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }

                Server.savedClients.Add(playerToLoad);
            }

            catch { }
        }

        public static void GiveSavedDataToPlayer(PlayerClient client)
        {
            var playerData = StaticProxy.playerHandler.AccountsHandler.GetAccount(client);
            if (playerData != null)
            {
                return;
            }

            client.Account = playerData;
            if (client.Account.Faction != null)
            {
                Faction factionToGive = Server.savedFactions.Find(fetch => fetch.name == client.Account.Faction.name);
                if (factionToGive != null) client.Account.Faction = factionToGive;
                else client.Account.Faction = null;
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
            if (client.Account.IsAdmin) return;

            int wealthToCompare = (int)Server.savedClients.Find(fetch => fetch.Account.Username == client.Account.Username).Account.Wealth;

            if (client.Account.Wealth - wealthToCompare > StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.BanThreshold && StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.BanThreshold > 0)
            {
                StaticProxy.playerHandler.AccountsHandler.SaveAccount(client);
                Server.savedClients.Find(fetch => fetch.Account.Username == client.Account.Username).Account.Wealth = client.Account.Wealth;
                Server.savedClients.Find(fetch => fetch.Account.Username == client.Account.Username).Account.PawnCount = client.Account.PawnCount;

                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "]'s Wealth Triggered Alarm [" + wealthToCompare + " > " + (int)client.Account.Wealth + "], Banning");
                Console.ForegroundColor = ConsoleColor.White;

                StaticProxy.playerHandler.BanlistHandler.BanPlayer(client, "Wealth Check triggered");
            }
            else if (client.Account.Wealth - wealthToCompare > StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.WarningThreshold && StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.WarningThreshold > 0)
            {
                StaticProxy.playerHandler.AccountsHandler.SaveAccount(client);
                Server.savedClients.Find(fetch => fetch.Account.Username == client.Account.Username).Account.Wealth = client.Account.Wealth;
                Server.savedClients.Find(fetch => fetch.Account.Username == client.Account.Username).Account.PawnCount = client.Account.PawnCount;

                Console.ForegroundColor = ConsoleColor.Yellow;
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "]'s Wealth Triggered Warning [" + wealthToCompare + " > " + (int)client.Account.Wealth + "]");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                StaticProxy.playerHandler.AccountsHandler.SaveAccount(client);
                Server.savedClients.Find(fetch => fetch.Account.Username == client.Account.Username).Account.Wealth = client.Account.Wealth;
                Server.savedClients.Find(fetch => fetch.Account.Username == client.Account.Username).Account.PawnCount = client.Account.PawnCount;
            }
        }

        public static bool CheckForConnectedPlayers(string tileID)
        {
            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient client in clients)
            {
                if (client.Account.HomeTileId == tileID) return true;
            }

            return false;
        }

        public static PlayerClient GetPlayerFromTile(string tileID)
        {
            return Networking.connectedClients.Find(fetch => fetch.Account.HomeTileId == tileID);
        }

        public static bool CheckForPlayerShield(string tileID)
        {
            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient client in clients)
            {
                if (client.Account.HomeTileId == tileID && !client.IsEventProtected && !client.Account.IsImmunized)
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
                if (client.Account.HomeTileId == tileID && !client.InRTSE && !client.Account.IsImmunized)
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
                if (client.Account.HomeTileId == tileID)
                {
                    string dataToReturn = client.Account.PawnCount.ToString() + "»" + client.Account.Wealth.ToString() + "»" + client.IsEventProtected + "»" + client.InRTSE;

                    if (client.Account.GiftString.Count > 0) dataToReturn += "»" + "True";
                    else dataToReturn += "»" + "False";

                    if (client.Account.TradeString.Count > 0) dataToReturn += "»" + "True";
                    else dataToReturn += "»" + "False";

                    Random rnd = new Random();
                    int chance = rnd.Next(0, 2);
                    if (chance == 1) Networking.SendData(client, "Spy│" + origin.Account.Username);

                    ConsoleUtils.LogToConsole("Spy Done Between [" + origin.Account.Username + "] And [" + client.Account.Username + "]");

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
                if (sc.Account.HomeTileId == data.Split('│')[2])
                {
                    ConsoleUtils.LogToConsole("Player [" + invoker.Account.Username + "] Has Sent Forced Event [" + data.Split('│')[1] + "] To [" + sc.Account.Username + "]");
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
                if (sc.Account.HomeTileId == tileToSend)
                {
                    Networking.SendData(sc, dataToSend);
                    ConsoleUtils.LogToConsole("Gift Done Between [" + invoker.Account.Username + "] And [" + sc.Account.Username + "]");
                    return;
                }
            }

            dataToSend = dataToSend.Replace("GiftedItems│", "");

            PlayerClient[] savedClients = Server.savedClients.ToArray();
            foreach (PlayerClient sc in savedClients)
            {
                if (sc.Account.HomeTileId == tileToSend)
                {
                    sc.Account.GiftString.Add(dataToSend);
                    StaticProxy.playerHandler.AccountsHandler.SaveAccount(sc);
                    ConsoleUtils.LogToConsole("Gift Done Between [" + invoker.Account.Username + "] And [" + sc.Account.Username + "] But Was Offline. Saving");
                    return;
                }
            }
        }

        public static void SendTradeRequestToPlayer(PlayerClient invoker, string data)
        {
            string dataToSend = "TradeRequest│" + invoker.Account.Username + "│" + data.Split('│')[2] + "│" + data.Split('│')[3];

            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient sc in clients)
            {
                if (sc.Account.HomeTileId == data.Split('│')[1])
                {
                    Networking.SendData(sc, dataToSend);
                    return;
                }
            }
        }

        public static void SendBarterRequestToPlayer(PlayerClient invoker, string data)
        {
            string dataToSend = "BarterRequest│" + invoker.Account.HomeTileId + "│" + data.Split('│')[2];

            PlayerClient[] clients = Networking.connectedClients.ToArray();
            foreach (PlayerClient sc in clients)
            {
                if (sc.Account.HomeTileId == data.Split('│')[1])
                {
                    Networking.SendData(sc, dataToSend);
                    return;
                }
            }
        }
    }
}
