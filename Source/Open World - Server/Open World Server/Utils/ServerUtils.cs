using System;
using System.Linq;
using System.Net;

namespace OpenWorldServer
{
    public static class ServerUtils
    {
        public static void CheckClientVersionRequirement()
        {
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Client Version Check:");
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                string version;

                WebClient wc = new WebClient();
                version = wc.DownloadString("https://raw.githubusercontent.com/TastyLollipop/OpenWorld/main/Latest%20Versions%20Cache");
                version = version.Split('│')[2].Replace("- Latest Client Version: ", "");
                version = version.Remove(0, 1);
                version = version.Remove(version.Count() - 1, 1);

                Server.latestClientVersion = version;

                ConsoleUtils.LogToConsole("Listening For Version [" + Server.latestClientVersion + "]");
            }

            catch
            {
                Console.ForegroundColor = ConsoleColor.White;
                ConsoleUtils.LogToConsole("Version Check Failed. This Is Not Dangerous");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void CheckServerVersion()
        {
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Server Version Check:");
            Console.ForegroundColor = ConsoleColor.White;

            string latestVersion = "";

            try
            {
                WebClient wc = new WebClient();
                latestVersion = wc.DownloadString("https://raw.githubusercontent.com/TastyLollipop/OpenWorld/main/Latest%20Versions%20Cache");
                latestVersion = latestVersion.Split('│')[1].Replace("- Latest Server Version: ", "");
                latestVersion = latestVersion.Remove(0, 1);
                latestVersion = latestVersion.Remove(latestVersion.Count() - 1, 1);
            }

            catch
            {
                Console.ForegroundColor = ConsoleColor.White;
                ConsoleUtils.LogToConsole("Version Check Failed. This is not dangerous");
                Console.ForegroundColor = ConsoleColor.White;
            }

            if (Server.serverVersion == latestVersion) ConsoleUtils.LogToConsole("Running Latest Version");
            else ConsoleUtils.LogToConsole("Running Outdated Or Unstable version. Please Update From Github At Earliest Convenience To Prevent Errors");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void SendChatMessage(PlayerClient client, string data)
        {
            string message = data.Split('│')[2];

            string messageForConsole = "Chat - [" + client.PlayerData.Username + "] " + message;

            ConsoleUtils.LogToConsole(messageForConsole);

            Server.chatCache.Add("[" + DateTime.Now + "]" + " │ " + messageForConsole);

            foreach (PlayerClient sc in Networking.connectedClients)
            {
                if (sc == client) continue;
                else Networking.SendData(sc, data);
            }
        }

        public static void SendPlayerListToAll(PlayerClient client)
        {
            foreach (PlayerClient sc in Networking.connectedClients)
            {
                if (sc == client) continue;

                SendPlayerList(sc);
            }
        }

        public static void SendPlayerList(PlayerClient client)
        {
            string playersToSend = GetPlayersToSend(client);
            Networking.SendData(client, playersToSend);
        }

        public static string GetPlayersToSend(PlayerClient client)
        {
            string dataToSend = "PlayerList│";

            if (Networking.connectedClients.Count == 0) return dataToSend;

            else
            {
                foreach (PlayerClient sc in Networking.connectedClients)
                {
                    if (sc == client) continue;

                    else dataToSend += sc.PlayerData.Username + ":";
                }

                dataToSend += "│" + Networking.connectedClients.Count();

                return dataToSend;
            }
        }
    }
}