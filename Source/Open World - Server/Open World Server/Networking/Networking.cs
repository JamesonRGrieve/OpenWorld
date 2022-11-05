using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace OpenWorldServer
{
    public static class Networking
    {
        public static IPAddress localAddress;
        public static int serverPort = 0;

        public static void ReadyServer()
        {
            ConsoleUtils.UpdateTitle();

            Threading.GenerateThreads(1);
            Threading.GenerateThreads(2);
        }

        public static void SendData(PlayerClient client, string data)
        {
            try
            {
                client.SendData(data);
            }
            catch
            {
                client.IsDisconnecting = true;
            }
        }

        public static void KickClients(PlayerClient client)
        {
            StaticProxy.playerHandler.RemovePlayer(client);

            client.Dispose();

            ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] has Disconnected");
        }

        public static void CheckClientsConnection()
        {
            ConsoleUtils.DisplayNetworkStatus();

            while (true)
            {

                PlayerClient[] actualClients = StaticProxy.playerHandler.ConnectedClients.ToArray();

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
