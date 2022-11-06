namespace OpenWorldServer
{
    public static class Networking
    {
        public static void ReadyServer()
        {
            ConsoleUtils.UpdateTitle();

            Threading.GenerateThreads(2);
            Threading.GenerateThreads(3);
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
