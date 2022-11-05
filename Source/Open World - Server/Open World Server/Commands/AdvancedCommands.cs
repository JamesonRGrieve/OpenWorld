using System;
using System.Collections.Generic;
using System.Linq;
using OpenWorldServer.Handlers.Old;

namespace OpenWorldServer
{
    public static class AdvancedCommands
    {
        public static string commandData;

        //Communication

        public static void SayCommand()
        {
            if (string.IsNullOrWhiteSpace(commandData))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                string messageForConsole = "Chat - [Console] " + commandData;

                ConsoleUtils.LogToConsole(messageForConsole);

                Server.chatCache.Add("[" + DateTime.Now + "]" + " │ " + messageForConsole);

                PlayerClient[] clients = StaticProxy.playerHandler.ConnectedClients.ToArray();
                foreach (PlayerClient sc in clients)
                {
                    Networking.SendData(sc, "ChatMessage│SERVER│" + commandData);
                }
            }
        }

        public static void BroadcastCommand()
        {
            if (string.IsNullOrWhiteSpace(commandData))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                PlayerClient[] clients = StaticProxy.playerHandler.ConnectedClients.ToArray();
                foreach (PlayerClient sc in clients)
                {
                    Networking.SendData(sc, "Notification│" + commandData);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Letter Sent To Every Connected Player");
                Console.WriteLine();
            }
        }

        public static void NotifyCommand()
        {
            bool isMissingParameters = false;

            string clientID = commandData.Split(' ')[0];
            string text = commandData.Replace(clientID + " ", "");

            if (string.IsNullOrWhiteSpace(clientID)) isMissingParameters = true;
            if (string.IsNullOrWhiteSpace(text)) isMissingParameters = true;

            if (isMissingParameters)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] not found");
                    Console.WriteLine();
                }

                else
                {
                    Networking.SendData(targetClient, "Notification│" + text);

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Sent Letter To [" + targetClient.Account.Username + "]");
                    Console.WriteLine();
                }
            }
        }

        //Items

        public static void GiveItemCommand()
        {
            Console.Clear();

            bool isMissingParameters = false;

            string clientID = commandData.Split(' ')[0];
            string itemID = commandData.Split(' ')[1];
            string itemQuantity = commandData.Split(' ')[2];
            string itemQuality = commandData.Split(' ')[3];

            if (string.IsNullOrWhiteSpace(clientID)) isMissingParameters = true;
            if (string.IsNullOrWhiteSpace(itemID)) isMissingParameters = true;
            if (string.IsNullOrWhiteSpace(itemQuantity)) isMissingParameters = true;
            if (string.IsNullOrWhiteSpace(itemQuality)) isMissingParameters = true;

            if (isMissingParameters)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                ConsoleUtils.WriteWithTime("Usage: Giveitem [username] [itemID] [itemQuantity] [itemQuality]");
                Console.WriteLine();
            }

            else
            {
                PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    Networking.SendData(targetClient, "GiftedItems│" + itemID + "┼" + itemQuantity + "┼" + itemQuality + "┼");

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Item Has Neen Gifted To Player [" + targetClient.Account.Username + "]");
                    Console.WriteLine();
                }
            }
        }

        public static void GiveItemAllCommand()
        {
            Console.Clear();

            bool isMissingParameters = false;

            string itemID = commandData.Split(' ')[0];
            string itemQuantity = commandData.Split(' ')[1];
            string itemQuality = commandData.Split(' ')[2];

            if (string.IsNullOrWhiteSpace(itemID)) isMissingParameters = true;
            if (string.IsNullOrWhiteSpace(itemQuantity)) isMissingParameters = true;
            if (string.IsNullOrWhiteSpace(itemQuality)) isMissingParameters = true;

            if (isMissingParameters)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                ConsoleUtils.WriteWithTime("Usage: Giveitemall [itemID] [itemQuantity] [itemQuality]");
                Console.WriteLine();
            }

            else
            {
                PlayerClient[] clients = StaticProxy.playerHandler.ConnectedClients.ToArray();
                foreach (PlayerClient client in clients)
                {
                    Networking.SendData(client, "GiftedItems│" + itemID + "┼" + itemQuantity + "┼" + itemQuality + "┼");

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Item Has Neen Gifted To All Players");
                    Console.WriteLine();
                }
            }
        }

        //Anti-PvP

        public static void ImmunizeCommand()
        {
            Console.Clear();

            string clientID = commandData.Split(' ')[0];

            if (string.IsNullOrWhiteSpace(clientID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    targetClient.Account.IsImmunized = true;
                    Server.savedClients.Find(fetch => fetch.Account.Username == targetClient.Account.Username).Account.IsImmunized = true;
                    StaticProxy.playerHandler.AccountsHandler.SaveAccount(targetClient);

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + targetClient.Account.Username + "] Has Been Inmmunized");
                    Console.WriteLine();
                }
            }
        }

        public static void DeimmunizeCommand()
        {
            Console.Clear();

            string clientID = commandData.Split(' ')[0];

            if (string.IsNullOrWhiteSpace(clientID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    targetClient.Account.IsImmunized = false;
                    Server.savedClients.Find(fetch => fetch.Account.Username == targetClient.Account.Username).Account.IsImmunized = false;
                    StaticProxy.playerHandler.AccountsHandler.SaveAccount(targetClient);

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + targetClient.Account.Username + "] Has Been Deinmmunized");
                    Console.WriteLine();
                }
            }
        }

        public static void ProtectCommand()
        {
            Console.Clear();

            string clientID = commandData.Split(' ')[0];

            if (string.IsNullOrWhiteSpace(clientID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    targetClient.IsEventProtected = true;
                    Server.savedClients.Find(fetch => fetch.Account.Username == targetClient.Account.Username).IsEventProtected = true;

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + targetClient.Account.Username + "] Has Been Protected");
                    Console.WriteLine();
                }
            }
        }

        public static void DeprotectCommand()
        {
            Console.Clear();

            string clientID = commandData.Split(' ')[0];

            if (string.IsNullOrWhiteSpace(clientID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    targetClient.IsEventProtected = false;
                    Server.savedClients.Find(fetch => fetch.Account.Username == targetClient.Account.Username).IsEventProtected = false;

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + targetClient.Account.Username + "] Has Been Deprotected");
                    Console.WriteLine();
                }
            }
        }

        //Events

        public static void InvokeCommand()
        {
            Console.Clear();

            bool isMissingParameters = false;

            string clientID = commandData.Split(' ')[0];
            string eventID = commandData.Split(' ')[1];

            if (string.IsNullOrWhiteSpace(clientID)) isMissingParameters = true;
            if (string.IsNullOrWhiteSpace(eventID)) isMissingParameters = true;

            if (isMissingParameters)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    Networking.SendData(targetClient, "ForcedEvent│" + eventID);

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Sent Event [" + eventID + "] to [" + targetClient.Account.Username + "]");
                    Console.WriteLine();
                }
            }
        }

        public static void PlagueCommand()
        {
            Console.Clear();

            string eventID = commandData.Split(' ')[0];

            if (string.IsNullOrWhiteSpace(eventID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            PlayerClient[] clients = StaticProxy.playerHandler.ConnectedClients.ToArray();
            foreach (PlayerClient client in clients)
            {
                Networking.SendData(client, "ForcedEvent│" + eventID);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.WriteWithTime("Sent Event [" + eventID + "] To Every Player");
            Console.WriteLine();
        }

        //Administration

        public static void PromoteCommand()
        {
            Console.Clear();

            string clientID = commandData.Split(' ')[0];

            if (string.IsNullOrWhiteSpace(clientID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    if (targetClient.Account.IsAdmin == true)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        ConsoleUtils.LogToConsole("Player [" + targetClient.Account.Username + "] Was Already An Administrator");
                        ConsoleUtils.LogToConsole(Environment.NewLine);
                    }

                    else
                    {
                        targetClient.Account.IsAdmin = true;
                        Server.savedClients.Find(fetch => fetch.Account.Username == clientID).Account.IsAdmin = true;
                        StaticProxy.playerHandler.AccountsHandler.SaveAccount(targetClient);

                        Networking.SendData(targetClient, "Admin│Promote");

                        Console.ForegroundColor = ConsoleColor.Green;
                        ConsoleUtils.LogToConsole("Player [" + targetClient.Account.Username + "] Has Been Promoted");
                        ConsoleUtils.LogToConsole(Environment.NewLine);
                    }
                }
            }
        }

        public static void DemoteCommand()
        {
            Console.Clear();

            string clientID = commandData.Split(' ')[0];

            if (string.IsNullOrWhiteSpace(clientID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    if (!targetClient.Account.IsAdmin)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        ConsoleUtils.LogToConsole("Player [" + targetClient.Account.Username + "] Is Not An Administrator");
                        ConsoleUtils.LogToConsole(Environment.NewLine);
                    }

                    else
                    {
                        targetClient.Account.IsAdmin = false;
                        Server.savedClients.Find(fetch => fetch.Account.Username == targetClient.Account.Username).Account.IsAdmin = false;
                        StaticProxy.playerHandler.AccountsHandler.SaveAccount(targetClient);

                        Networking.SendData(targetClient, "Admin│Demote");

                        Console.ForegroundColor = ConsoleColor.Green;
                        ConsoleUtils.LogToConsole("Player [" + targetClient.Account.Username + "] Has Been Demoted");
                        ConsoleUtils.LogToConsole(Environment.NewLine);
                    }
                }
            }
        }

        public static void PlayerDetailsCommand()
        {
            Console.Clear();

            string clientID = commandData.Split(' ')[0];
            PlayerClient clientToInvestigate = null;

            if (string.IsNullOrWhiteSpace(clientID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                PlayerClient targetClient = Server.savedClients.Find(fetch => fetch.Account.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    bool isConnected = false;
                    string ip = "None";

                    if (StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == targetClient.Account.Username) != null)
                    {
                        clientToInvestigate = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == targetClient.Account.Username);
                        isConnected = true;
                        ip = clientToInvestigate.IPAddress.ToString();
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player Details: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    ConsoleUtils.WriteWithTime("Username: [" + targetClient.Account.Username + "]");
                    ConsoleUtils.WriteWithTime("Password: [" + targetClient.Account.Password + "]");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Security: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    ConsoleUtils.WriteWithTime("Connection IP: [" + ip + "]");
                    ConsoleUtils.WriteWithTime("Admin: [" + targetClient.Account.IsAdmin + "]");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Status: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    ConsoleUtils.WriteWithTime("Online: [" + isConnected + "]");
                    ConsoleUtils.WriteWithTime("Immunized: [" + targetClient.Account.IsImmunized + "]");
                    ConsoleUtils.WriteWithTime("Event Shielded: [" + targetClient.IsEventProtected + "]");
                    ConsoleUtils.WriteWithTime("In RTSE: [" + (targetClient.RtsActionPartner != null).ToString() + "]");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Wealth: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    ConsoleUtils.WriteWithTime("Stored Gifts: [" + targetClient.Account.GiftString.Count + "]");
                    ConsoleUtils.WriteWithTime("Stored Trades: [" + targetClient.Account.TradeString.Count + "]");
                    ConsoleUtils.WriteWithTime("Wealth Value: [" + targetClient.Account.Wealth + "]");
                    ConsoleUtils.WriteWithTime("Pawn Count: [" + targetClient.Account.PawnCount + "]");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Details: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    ConsoleUtils.WriteWithTime("Home Tile ID: [" + targetClient.Account.HomeTileId + "]");
                    ConsoleUtils.WriteWithTime("Faction: [" + (targetClient.Account.Faction == null ? "None" : targetClient.Account.Faction.name) + "]");
                    Console.WriteLine();
                }
            }
        }

        public static void FactionDetailsCommand()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;

            string factionID = commandData.Split(' ')[0];
            if (string.IsNullOrWhiteSpace(factionID))
            {
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                Faction factionToSearch = Server.savedFactions.Find(fetch => fetch.name == commandData);

                if (factionToSearch == null)
                {
                    ConsoleUtils.WriteWithTime("Faction " + commandData + " Was Not Found");
                    Console.WriteLine();
                }

                else
                {
                    ConsoleUtils.WriteWithTime("Faction Details Of [" + factionToSearch.name + "]:");
                    Console.WriteLine();

                    ConsoleUtils.WriteWithTime("Members:");
                    Console.ForegroundColor = ConsoleColor.White;

                    foreach (KeyValuePair<PlayerClient, FactionHandler.MemberRank> member in factionToSearch.members)
                    {
                        ConsoleUtils.WriteWithTime("[" + member.Value + "]" + " - " + member.Key.Account.Username);
                    }

                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Wealth:");
                    Console.ForegroundColor = ConsoleColor.White;

                    ConsoleUtils.WriteWithTime(factionToSearch.wealth.ToString());
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Structures:");
                    Console.ForegroundColor = ConsoleColor.White;

                    if (factionToSearch.factionStructures.Count == 0)
                        ConsoleUtils.WriteWithTime("No Structures");
                    else
                    {
                        FactionStructure[] structures = factionToSearch.factionStructures.ToArray();
                        foreach (FactionStructure structure in structures)
                        {
                            ConsoleUtils.WriteWithTime("[" + structure.structureTile + "]" + " - " + structure.structureName);
                        }
                    }

                    Console.WriteLine();
                }
            }
        }

        //Security

        public static void BanCommand()
        {
            Console.Clear();

            string clientID = commandData.Split(' ')[0];

            if (string.IsNullOrWhiteSpace(clientID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    StaticProxy.playerHandler.BanlistHandler.BanPlayer(targetClient);
                    ConsoleUtils.LogToConsole(Environment.NewLine);
                }
            }
        }

        public static void PardonCommand()
        {
            string clientID = commandData.Split(' ')[0];

            if (string.IsNullOrWhiteSpace(clientID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                var banInfo = StaticProxy.playerHandler.BanlistHandler.GetBanInfo(clientID);
                if (banInfo != null)
                {
                    StaticProxy.playerHandler.BanlistHandler.UnbanPlayer(banInfo);
                    ConsoleUtils.LogToConsole(Environment.NewLine);
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                Console.WriteLine();
            }
        }

        public static void KickCommand()
        {
            Console.Clear();

            string clientID = commandData.Split(' ')[0];

            if (string.IsNullOrWhiteSpace(clientID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    targetClient.IsDisconnecting = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + targetClient.Account.Username + "] Has Been Kicked");
                    Console.WriteLine();
                }
            }
        }
    }
}