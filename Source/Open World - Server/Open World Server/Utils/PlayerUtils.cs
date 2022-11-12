using System;
using System.IO;
using System.Linq;

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
                //PlayerClient playerToLoad = (PlayerClient)obj;

                //s.Flush();
                //s.Close();
                //s.Dispose();



                var account = StaticProxy.playerManager.AccountsHandler.GetAccount(Path.GetFileNameWithoutExtension(path));
                if (account == null)
                    return;

                PlayerClient playerToLoad = new PlayerClient(null) { Account = account };

                if (!string.IsNullOrWhiteSpace(playerToLoad.Account.HomeTileId))
                {
                    try
                    {
                        //Server.savedSettlements.Add(playerToLoad.Account.HomeTileId, new List<string>() { playerToLoad.Account.Username });
                    }
                    catch
                    {
                        playerToLoad.Account.HomeTileId = null;
                        StaticProxy.playerManager.AccountsHandler.SaveAccount(playerToLoad.Account);

                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Error! Player " + playerToLoad.Account.Username + " Is Using A Cloned Entry! Fixing");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }

                if (playerToLoad.Account.Faction != null)
                {
                    FactionOld factionToFech = Server.savedFactions.Find(fetch => fetch.name == playerToLoad.Account.Faction.name);
                    if (factionToFech == null)
                    {
                        playerToLoad.Account.Faction = null;
                        StaticProxy.playerManager.AccountsHandler.SaveAccount(playerToLoad.Account);

                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Error! Player " + playerToLoad.Account.Username + " Is Using A Missing Faction! Fixing");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }

                //Server.savedClients.Add(playerToLoad);
            }

            catch { }
        }


        public static void CheckForPlayerWealth(PlayerClient client, float oldWealth)
        {
            if (StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.IsActive == false)
            {
                return;
            }

            if (StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.BanThreshold == 0 &&
                StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.WarningThreshold == 0)
            {
                return;
            }

            if (client.Account.IsAdmin)
            {
                return;
            }

            var difInWelath = client.Account.Wealth - oldWealth;
            if (StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.BanThreshold > 0 &&
                difInWelath > StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.BanThreshold)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "]'s Wealth Triggered Alarm [" + oldWealth + " > " + (int)client.Account.Wealth + "], Banning");
                Console.ForegroundColor = ConsoleColor.White;

                StaticProxy.playerManager.BanlistHandler.BanPlayer(client, "Wealth Check triggered");
            }
            else if (StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.WarningThreshold > 0 &&
                difInWelath > StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.WarningThreshold)
            {

                Console.ForegroundColor = ConsoleColor.Yellow;
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "]'s Wealth Triggered Warning [" + oldWealth + " > " + (int)client.Account.Wealth + "]");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static bool CheckForConnectedPlayers(string tileID)
        {
            var clients = StaticProxy.playerManager.ConnectedClients;
            foreach (PlayerClient client in clients)
            {
                if (client.Account.HomeTileId == tileID) return true;
            }

            return false;
        }

        public static PlayerClient GetPlayerFromTile(string tileID)
        {
            return StaticProxy.playerManager.ConnectedClients.FirstOrDefault(fetch => fetch.Account.HomeTileId == tileID);
        }

        public static bool CheckForPlayerShield(string tileID)
        {
            var clients = StaticProxy.playerManager.ConnectedClients;
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
            foreach (PlayerClient client in StaticProxy.playerManager.ConnectedClients)
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
            var clients = StaticProxy.playerManager.ConnectedClients;
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

            var clients = StaticProxy.playerManager.ConnectedClients;
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

            var clients = StaticProxy.playerManager.ConnectedClients;
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

            var savedClients = StaticProxy.playerManager.AccountsHandler.Accounts;
            foreach (var sc in savedClients)
            {
                if (sc.HomeTileId == tileToSend)
                {
                    sc.GiftString.Add(dataToSend);
                    StaticProxy.playerManager.AccountsHandler.SaveAccount(sc);
                    ConsoleUtils.LogToConsole("Gift Done Between [" + invoker.Account.Username + "] And [" + sc.Username + "] But Was Offline. Saving");
                    return;
                }
            }
        }

        public static void SendTradeRequestToPlayer(PlayerClient invoker, string data)
        {
            string dataToSend = "TradeRequest│" + invoker.Account.Username + "│" + data.Split('│')[2] + "│" + data.Split('│')[3];

            var clients = StaticProxy.playerManager.ConnectedClients;
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

            var clients = StaticProxy.playerManager.ConnectedClients;
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
