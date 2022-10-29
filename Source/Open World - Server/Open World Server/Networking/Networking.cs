﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OpenWorldServer
{
    public static class Networking
    {
        private static TcpListener server;
        public static IPAddress localAddress;
        public static int serverPort = 0;

        public static List<ServerClient> connectedClients = new List<ServerClient>();

        public static void ReadyServer()
        {
            server = new TcpListener(localAddress, serverPort);
            server.Start();

            ConsoleUtils.UpdateTitle();

            Threading.GenerateThreads(1);
            Threading.GenerateThreads(2);

            ListenForIncomingUsers();
        }

        private static void ListenForIncomingUsers()
        {
            server.BeginAcceptTcpClient(AcceptClients, server);
        }

        private static void AcceptClients(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;

            ServerClient newServerClient = new ServerClient(listener.EndAcceptTcpClient(ar));

            connectedClients.Add(newServerClient);

            Threading.GenerateClientThread(newServerClient);

            ListenForIncomingUsers();
        }

        public static void ListenToClient(ServerClient client)
        {
            NetworkStream s = client.tcp.GetStream();
            StreamWriter sw = new StreamWriter(s);
            StreamReader sr = new StreamReader(s, true);

            while (true)
            {
                Thread.Sleep(1);

                try
                {
                    if (client.disconnectFlag) return;

                    if (!s.DataAvailable) continue;

                    string encryptedData = sr.ReadLine();
                    string data = Encryption.DecryptString(encryptedData);
                    //if (data != "Ping") Debug.WriteLine(data);

                    if (encryptedData != null)
                    {
                        if (data.StartsWith("Connect│"))
                        {
                            NetworkingHandler.ConnectHandle(client, data);
                        }

                        else if (data.StartsWith("ChatMessage│"))
                        {
                            NetworkingHandler.ChatMessageHandle(client, data);
                        }

                        else if (data.StartsWith("UserSettlement│"))
                        {
                            NetworkingHandler.UserSettlementHandle(client, data);
                        }

                        else if (data.StartsWith("ForceEvent│"))
                        {
                            NetworkingHandler.ForceEventHandle(client, data);
                        }

                        else if (data.StartsWith("SendGiftTo│"))
                        {
                            NetworkingHandler.SendGiftHandle(client, data);
                        }

                        else if (data.StartsWith("SendTradeTo│"))
                        {
                            NetworkingHandler.SendTradeHandle(client, data);
                        }

                        else if (data.StartsWith("SendBarterTo│"))
                        {
                            NetworkingHandler.SendBarterHandle(client, data);
                        }

                        else if (data.StartsWith("TradeStatus│"))
                        {
                            NetworkingHandler.TradeStatusHandle(client, data);
                        }

                        else if (data.StartsWith("BarterStatus│"))
                        {
                            NetworkingHandler.BarterStatusHandle(client, data);
                        }

                        else if (data.StartsWith("GetSpyInfo│"))
                        {
                            NetworkingHandler.SpyInfoHandle(client, data);
                        }

                        else if (data.StartsWith("FactionManagement│"))
                        {
                            NetworkingHandler.FactionManagementHandle(client, data);
                        }
                    }
                }

                catch
                {
                    client.disconnectFlag = true;
                    return;
                }
            }
        }

        public static void SendData(ServerClient client, string data)
        {
            if (!client.tcp.Connected) client.disconnectFlag = true;
            else
            {
                try
                {
                    NetworkStream s = client.tcp.GetStream();
                    StreamWriter sw = new StreamWriter(s);

                    sw.WriteLine(Encryption.EncryptString(data));
                    sw.Flush();
                }
                catch { }
            }
        }

        public static void KickClients(ServerClient client, string kickMode)
        {
            try { client.tcp.Close(); }
            catch { }

            connectedClients.Remove(client);

            if (kickMode == "Normal") ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] Has Disconnected");
            else if (kickMode == "Silent") { }

            ServerUtils.SendPlayerListToAll(null);

            ConsoleUtils.UpdateTitle();
        }

        public static void CheckClientsConnection()
        {
            ConsoleUtils.DisplayNetworkStatus();

            while (true)
            {
                Thread.Sleep(500);

                try
                {
                    List<ServerClient> actualClients = new List<ServerClient>();
                    actualClients.AddRange(connectedClients);

                    List<ServerClient> clientsToDisconnect = new List<ServerClient>();

                    foreach (ServerClient client in actualClients)
                    {
                        if (client.disconnectFlag) clientsToDisconnect.Add(client);

                        Thread.Sleep(1);

                        SendData(client, "Ping");
                    }

                    foreach (ServerClient client in clientsToDisconnect)
                    {
                        Thread.Sleep(1);

                        KickClients(client, "Normal");
                    }
                }

                catch { continue; }
            }
        }
    }
}
