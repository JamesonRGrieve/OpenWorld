using System;
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

        public bool DataAvailable => !this.isReceiving && (this.tcpClient?.GetStream()?.DataAvailable ?? false);

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
            this.Account = new PlayerData();
        }

        public void SendData(string data)
        {
            if (!this.IsConnected)
            {
                ConsoleUtils.LogToConsole($"Can't send Data to [{this.Account.Username}] since Player is not connected", ConsoleColor.Yellow);
                return;
            }

            var encryptedData = Encryption.EncryptString(data);
            try
            {
                var sw = new StreamWriter(this.tcpClient.GetStream());
                sw.WriteLine(encryptedData);
                sw.Flush();
            }
            catch (Exception ex)
            {
                // depending on the error we could catch explicit exceptions and target client to disconnect
                // for now we just log them
                Console.WriteLine($"Error Sanding Data by Player [{this.Account.Username}]:", ConsoleColor.Red);
                Console.WriteLine(ex.Message, ConsoleColor.Red);
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
                    var sr = new StreamReader(this.tcpClient.GetStream(), true);
                    var encryptedData = sr.ReadLine();
                    data = Encryption.DecryptString(encryptedData);
                }

                return data;
            }
            catch
            {
                return null;
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
