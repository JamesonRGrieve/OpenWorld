using System;
using System.IO;
using System.Linq;
using System.Net;
using OpenWorldServer.Data;
using OpenWorldServer.Utils;

namespace OpenWorldServer
{
    public static class ServerUtils
    {


        public static void CheckServerVersion()
        {
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Version Check:");
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

        public static void SendChatMessage(ServerClient client, string data)
        {
            string message = data.Split('│')[2];

            string messageForConsole = "Chat - [" + client.username + "] " + message;

            ConsoleUtils.LogToConsole(messageForConsole);

            Server.chatCache.Add("[" + DateTime.Now + "]" + " │ " + messageForConsole);

            foreach (ServerClient sc in Networking.connectedClients)
            {
                if (sc == client) continue;
                else Networking.SendData(sc, data);
            }
        }

        public static void SendPlayerListToAll(ServerClient client)
        {
            foreach (ServerClient sc in Networking.connectedClients)
            {
                if (sc == client) continue;

                SendPlayerList(sc);
            }
        }

        public static void SendPlayerList(ServerClient client)
        {
            string playersToSend = GetPlayersToSend(client);
            Networking.SendData(client, playersToSend);
        }

        public static string GetPlayersToSend(ServerClient client)
        {
            string dataToSend = "PlayerList│";

            if (Networking.connectedClients.Count == 0) return dataToSend;

            else
            {
                foreach (ServerClient sc in Networking.connectedClients)
                {
                    if (sc == client) continue;

                    else dataToSend += sc.username + ":";
                }

                dataToSend += "│" + Networking.connectedClients.Count();

                return dataToSend;
            }
        }

        internal static ServerConfig LoadServerConfig(string filePath)
        {
            var config = new ServerConfig();

            Console.WriteLine();
            ConsoleUtils.LogToConsole("Loading Server Settings", ConsoleColor.Green);

            if (File.Exists(filePath))
            {
                try
                {
                    config = JsonDataHelper.Load<ServerConfig>(filePath);
                }
                catch (Exception ex)
                {
                    // Possible error would be incorrect data
                    ConsoleUtils.LogToConsole("Error while loading Server Settings:", ConsoleColor.Red);
                    ConsoleUtils.LogToConsole(ex.Message, ConsoleColor.Red);

                    return null;
                }
            }
            else
            {
                ConsoleUtils.LogToConsole("No Server Settings File found, generating new one", ConsoleColor.Yellow);
                JsonDataHelper.Save(config, filePath);
            }

            ConsoleUtils.LogToConsole("Loaded Server Settings");
            return config;
        }
    }
}