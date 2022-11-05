using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using OpenWorld.Shared.Networking.Packets;
using OpenWorldServer.Data;

namespace OpenWorldServer
{
    public class PlayerClient
    {
        private readonly TcpClient tcpClient;

        public IPAddress IPAddress => ((IPEndPoint)this.tcpClient.Client.RemoteEndPoint).Address;

        public bool DataAvailable => !this.isReceiving && this.IsConnected && (this.tcpClient?.GetStream()?.DataAvailable ?? false);

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

        public void SendData(IPacket packet) => this.SendData(packet.GetData());

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
                ConsoleUtils.LogToConsole($"Error sending Data to Player [{this.Account.Username}] ({ex.GetType().Name}):", ConsoleColor.Red);
                ConsoleUtils.LogToConsole(ex.Message, ConsoleColor.Red);
                this.IsDisconnecting = true;
            }
        }

        public string ReceiveData()
        {
            try
            {
                this.isReceiving = true;

                string data = null;
                if (this.IsConnected && !this.IsDisconnecting)
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
