using System.Threading;
using OpenWorld.Server.Handlers.Old;

namespace OpenWorld.Server
{
    public static class Networking
    {
        public static void ReadyServer()
        {
            GenerateThreads(2);
            GenerateThreads(3);
        }

        private static void GenerateThreads(int threadID)
        {
            if (threadID == 2)
            {
                Thread CheckThread = new Thread(() => FactionProductionSiteHandler.TickProduction());
                CheckThread.IsBackground = true;
                CheckThread.Name = "Factions Production Site Tick Thread";
                CheckThread.Start();
            }

            else if (threadID == 3)
            {
                Thread CheckThread = new Thread(() => FactionBankHandler.TickBank());
                CheckThread.IsBackground = true;
                CheckThread.Name = "Factions Bank Tick Thread";
                CheckThread.Start();
            }

            else return;
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
    }
}
