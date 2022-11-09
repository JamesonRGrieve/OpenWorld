using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using OpenWorld.Shared.Enums;
using OpenWorld.Shared.Networking.Packets;
using OpenWorldServer.Data;

namespace OpenWorldServer
{
    public class PlayerClient
    {
        private readonly TcpClient tcpClient;

        public IPAddress IPAddress => ((IPEndPoint)this.tcpClient.Client.RemoteEndPoint).Address;

        public bool DataAvailable => !this.isReceiving && this.IsConnected && (this.tcpClient?.GetStream()?.DataAvailable ?? false);

        public bool IsConnected => this.CheckConnection();

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
                this.IsDisconnecting = true;
                return;
            }

            var encryptedData = Encryption.EncryptString(data);
            try
            {
                var sw = new StreamWriter(this.tcpClient.GetStream());
                sw.WriteLine(encryptedData);
                sw.Flush();
            }
            catch (IOException)
            {
                // IOException means the client probably lost connection
                this.IsDisconnecting = true;
            }
            catch (Exception ex)
            {
                ConsoleUtils.LogToConsole($"Error sending Data to Player [{this.Account.Username}] ({ex.GetType().Name}):", ConsoleUtils.ConsoleLogMode.Error);
                ConsoleUtils.LogToConsole(ex.Message, ConsoleUtils.ConsoleLogMode.Error);
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

        public void Disconnect(DisconnectReason reason)
        {
            this.SendData(new DisconnectPacket(reason));
            this.IsDisconnecting = true;
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

        private bool CheckConnection()
        {
            try
            {
                if (this.tcpClient != null && this.tcpClient.Client != null && this.tcpClient.Client.Connected)
                {
                    if (this.tcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (this.tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            catch
            {
            }
            return false;
        }
    }
}
