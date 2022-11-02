using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using OpenWorld.Shared.Networking;
using OpenWorld.Shared.Networking.Packets;

namespace OpenWorldServer
{
    public static class Networking
    {
        private static TcpListener server;
        public static IPAddress localAddress;
        public static int serverPort = 0;

        public static List<PlayerClient> connectedClients = new List<PlayerClient>();

        public static void ReadyServer()
        {
            server = new TcpListener(localAddress, serverPort);
            server.Start();

            ConsoleUtils.UpdateTitle();

            Threading.GenerateThreads(1);
            Threading.GenerateThreads(2);

            while (true) ListenForIncomingUsers();
        }

        private static void ListenForIncomingUsers()
        {
            PlayerClient newServerClient = new PlayerClient(server.AcceptTcpClient());

            connectedClients.Add(newServerClient);

            Threading.GenerateClientThread(newServerClient);
        }

        public static void ListenToClient(PlayerClient client)
        {
            NetworkStream s = client.ClientStream;
            StreamReader sr = new StreamReader(s, true);

            while (true)
            {
                try
                {
                    if (client.IsDisconnecting) return;

                    string encryptedData = sr.ReadLine();
                    string data = Encryption.DecryptString(encryptedData);

                    if (data == null)
                    {
                        client.IsDisconnecting = true;
                        return;
                    }

                    //if (data != "Ping") Debug.WriteLine(data);

                    if (data.StartsWith("Connect│"))
                    {
                        NetworkingHandler.ConnectHandle(client, PacketHandler.GetPacket<ConnectPacket>(data));
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

                catch
                {
                    client.IsDisconnecting = true;
                    return;
                }
            }
        }

        public static void SendData(PlayerClient client, string data)
        {
            try
            {
                NetworkStream s = client.ClientStream;
                StreamWriter sw = new StreamWriter(s);

                sw.WriteLine(Encryption.EncryptString(data));
                sw.Flush();
            }
            catch { client.IsDisconnecting = true; }
        }

        public static void KickClients(PlayerClient client)
        {
            connectedClients.Remove(client);

            client.Dispose();

            ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] has Disconnected");
        }

        public static void CheckClientsConnection()
        {
            ConsoleUtils.DisplayNetworkStatus();

            while (true)
            {

                PlayerClient[] actualClients = connectedClients.ToArray();

                List<PlayerClient> clientsToDisconnect = new List<PlayerClient>();

                foreach (PlayerClient client in actualClients)
                {
                    if (client.IsDisconnecting)
                        clientsToDisconnect.Add(client);
                    else
                        SendData(client, "Ping");
                }

                foreach (PlayerClient client in clientsToDisconnect)
                {
                    KickClients(client);
                }

                if (clientsToDisconnect.Count > 0)
                {
                    ConsoleUtils.UpdateTitle();
                    ServerUtils.SendPlayerListToAll(null);
                }
            }
        }
    }
}
