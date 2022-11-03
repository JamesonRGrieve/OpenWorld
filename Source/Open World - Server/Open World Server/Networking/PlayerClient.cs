using System.IO;
using System.Net;
using System.Net.Sockets;
using OpenWorldServer.Data;

namespace OpenWorldServer
{
    public class PlayerClient
    {
        private readonly TcpClient tcpClient;

        public IPAddress IPAddress => ((IPEndPoint)this.tcpClient.Client.RemoteEndPoint).Address;

        public bool DataAvailable => !this.isReceiving && (this.tcpClient.GetStream()?.DataAvailable ?? false);

        public bool IsConnected => this.tcpClient != null && this.tcpClient.Connected;

        public bool IsLoggedIn { get; set; } = false;

        public bool IsDisconnecting { get; set; } = false;

        public PlayerData Account { get; set; }

        public bool IsEventProtected { get; set; } = false;

        public PlayerClient RtsActionPartner { get; set; }

        public bool InRTSE { get; set; } = false;

        private bool isReceiving = false;

        public PlayerClient(TcpClient userSocket)
        {
            this.tcpClient = userSocket;
            if (this.tcpClient != null)
            {
                this.tcpClient.NoDelay = false;
            }
            this.Account = new PlayerData();
        }

        public void SendData(string data)
        {
            var encryptedData = Encryption.EncryptString(data);
            try
            {
                var sw = new StreamWriter(this.tcpClient.GetStream());
                sw.WriteLine(encryptedData);
                sw.Flush();
            }
            catch
            {
            }
        }

        public string ReceiveData()
        {
            try
            {
                this.isReceiving = true;

                string data = null;
                if (this.IsConnected)
                {
                    string encryptedData = null;
                    try
                    {
                        var sr = new StreamReader(this.tcpClient.GetStream(), true);
                        encryptedData = sr.ReadLine();
                    }
                    catch
                    {
                    }

                    data = Encryption.DecryptString(encryptedData);
                }

                return data;
            }
            finally
            {

                this.isReceiving = false;
            }
        }

        public void Dispose()
        {
            if (this.tcpClient?.Connected ?? false)
            {
                try
                {
                    this.tcpClient.Close();
                }
                catch
                {
                }

                this.tcpClient.Dispose();
            }
        }
    }
}
