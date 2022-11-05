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

        public static void CheckClientsConnection()
        {
            ConsoleUtils.DisplayNetworkStatus();

            while (true)
            {

                PlayerClient[] actualClients = StaticProxy.playerHandler.ConnectedClients.ToArray();

                List<PlayerClient> clientsToDisconnect = new List<PlayerClient>();

                foreach (PlayerClient client in actualClients)
                {
                    if (client != null && !client.IsDisconnecting)
                        SendData(client, "Ping");
                }
            }
        }
    }
}
