using System;
using System.Collections.Generic;
using System.Net;

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

                foreach (ServerClient sc in Networking.connectedClients)
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
                foreach (ServerClient sc in Networking.connectedClients)
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
                ServerClient targetClient = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == clientID);

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
                    ConsoleUtils.WriteWithTime("Sent Letter To [" + targetClient.PlayerData.Username + "]");
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
                ServerClient targetClient = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == clientID);

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
                    ConsoleUtils.WriteWithTime("Item Has Neen Gifted To Player [" + targetClient.PlayerData.Username + "]");
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
                foreach (ServerClient client in Networking.connectedClients)
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
                ServerClient targetClient = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    targetClient.PlayerData.IsImmunized = true;
                    Server.savedClients.Find(fetch => fetch.PlayerData.Username == targetClient.PlayerData.Username).PlayerData.IsImmunized = true;
                    StaticProxy.playerHandler.SavePlayerData(targetClient);

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + targetClient.PlayerData.Username + "] Has Been Inmmunized");
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
                ServerClient targetClient = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    targetClient.PlayerData.IsImmunized = false;
                    Server.savedClients.Find(fetch => fetch.PlayerData.Username == targetClient.PlayerData.Username).PlayerData.IsImmunized = false;
                    StaticProxy.playerHandler.SavePlayerData(targetClient);

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + targetClient.PlayerData.Username + "] Has Been Deinmmunized");
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
                ServerClient targetClient = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    targetClient.eventShielded = true;
                    Server.savedClients.Find(fetch => fetch.PlayerData.Username == targetClient.PlayerData.Username).eventShielded = true;

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + targetClient.PlayerData.Username + "] Has Been Protected");
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
                ServerClient targetClient = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    targetClient.eventShielded = false;
                    Server.savedClients.Find(fetch => fetch.PlayerData.Username == targetClient.PlayerData.Username).eventShielded = false;

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + targetClient.PlayerData.Username + "] Has Been Deprotected");
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
            ServerClient target = null;

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
                ServerClient targetClient = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    Networking.SendData(target, "ForcedEvent│" + eventID);

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Sent Event [" + eventID + "] to [" + targetClient.PlayerData.Username + "]");
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

            foreach (ServerClient client in Networking.connectedClients)
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
                ServerClient targetClient = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    if (targetClient.PlayerData.IsAdmin == true)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        ConsoleUtils.LogToConsole("Player [" + targetClient.PlayerData.Username + "] Was Already An Administrator");
                        ConsoleUtils.LogToConsole(Environment.NewLine);
                    }

                    else
                    {
                        targetClient.PlayerData.IsAdmin = true;
                        Server.savedClients.Find(fetch => fetch.PlayerData.Username == clientID).PlayerData.IsAdmin = true;
                        StaticProxy.playerHandler.SavePlayerData(targetClient);

                        Networking.SendData(targetClient, "Admin│Promote");

                        Console.ForegroundColor = ConsoleColor.Green;
                        ConsoleUtils.LogToConsole("Player [" + targetClient.PlayerData.Username + "] Has Been Promoted");
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
                ServerClient targetClient = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    if (!targetClient.PlayerData.IsAdmin)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        ConsoleUtils.LogToConsole("Player [" + targetClient.PlayerData.Username + "] Is Not An Administrator");
                        ConsoleUtils.LogToConsole(Environment.NewLine);
                    }

                    else
                    {
                        targetClient.PlayerData.IsAdmin = false;
                        Server.savedClients.Find(fetch => fetch.PlayerData.Username == targetClient.PlayerData.Username).PlayerData.IsAdmin = false;
                        StaticProxy.playerHandler.SavePlayerData(targetClient);

                        Networking.SendData(targetClient, "Admin│Demote");

                        Console.ForegroundColor = ConsoleColor.Green;
                        ConsoleUtils.LogToConsole("Player [" + targetClient.PlayerData.Username + "] Has Been Demoted");
                        ConsoleUtils.LogToConsole(Environment.NewLine);
                    }
                }
            }
        }

        public static void PlayerDetailsCommand()
        {
            Console.Clear();

            string clientID = commandData.Split(' ')[0];
            ServerClient clientToInvestigate = null;

            if (string.IsNullOrWhiteSpace(clientID))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                ConsoleUtils.WriteWithTime("Missing Parameters");
                Console.WriteLine();
            }

            else
            {
                ServerClient targetClient = Server.savedClients.Find(fetch => fetch.PlayerData.Username == clientID);

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

                    if (Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == targetClient.PlayerData.Username) != null)
                    {
                        clientToInvestigate = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == targetClient.PlayerData.Username);
                        isConnected = true;
                        ip = ((IPEndPoint)clientToInvestigate.tcp.Client.RemoteEndPoint).Address.ToString();
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player Details: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    ConsoleUtils.WriteWithTime("Username: [" + targetClient.PlayerData.Username + "]");
                    ConsoleUtils.WriteWithTime("Password: [" + targetClient.PlayerData.Password + "]");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Security: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    ConsoleUtils.WriteWithTime("Connection IP: [" + ip + "]");
                    ConsoleUtils.WriteWithTime("Admin: [" + targetClient.PlayerData.IsAdmin + "]");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Status: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    ConsoleUtils.WriteWithTime("Online: [" + isConnected + "]");
                    ConsoleUtils.WriteWithTime("Immunized: [" + targetClient.PlayerData.IsImmunized + "]");
                    ConsoleUtils.WriteWithTime("Event Shielded: [" + targetClient.eventShielded + "]");
                    ConsoleUtils.WriteWithTime("In RTSE: [" + targetClient.inRTSE + "]");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Wealth: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    ConsoleUtils.WriteWithTime("Stored Gifts: [" + targetClient.PlayerData.GiftString.Count + "]");
                    ConsoleUtils.WriteWithTime("Stored Trades: [" + targetClient.PlayerData.TradeString.Count + "]");
                    ConsoleUtils.WriteWithTime("Wealth Value: [" + targetClient.PlayerData.Wealth + "]");
                    ConsoleUtils.WriteWithTime("Pawn Count: [" + targetClient.PlayerData.PawnCount + "]");
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Details: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    ConsoleUtils.WriteWithTime("Home Tile ID: [" + targetClient.PlayerData.HomeTileId + "]");
                    ConsoleUtils.WriteWithTime("Faction: [" + (targetClient.PlayerData.Faction == null ? "None" : targetClient.PlayerData.Faction.name) + "]");
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

                    foreach (KeyValuePair<ServerClient, FactionHandler.MemberRank> member in factionToSearch.members)
                    {
                        ConsoleUtils.WriteWithTime("[" + member.Value + "]" + " - " + member.Key.PlayerData.Username);
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

                    if (factionToSearch.factionStructures.Count == 0) ConsoleUtils.WriteWithTime("No Structures");
                    else foreach (FactionStructure structure in factionToSearch.factionStructures)
                        {
                            ConsoleUtils.WriteWithTime("[" + structure.structureTile + "]" + " - " + structure.structureName);
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
                ServerClient targetClient = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    StaticProxy.playerHandler.BanPlayer(targetClient);
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
                var banInfo = StaticProxy.playerHandler.GetBanInfo(clientID);
                if (banInfo != null)
                {
                    StaticProxy.playerHandler.UnbanPlayer(banInfo);
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
                ServerClient targetClient = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == clientID);

                if (targetClient == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + clientID + "] Not Found");
                    Console.WriteLine();
                }

                else
                {
                    targetClient.disconnectFlag = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    ConsoleUtils.WriteWithTime("Player [" + targetClient.PlayerData.Username + "] Has Been Kicked");
                    Console.WriteLine();
                }
            }
        }
    }
}