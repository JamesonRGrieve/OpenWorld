﻿using System.IO;
using System.Net;
using System.Net.Sockets;
using OpenWorldServer.Data;

namespace OpenWorldServer
{
    public class PlayerClient
    {
        private readonly TcpClient tcpClient;

        public IPAddress IPAddress => ((IPEndPoint)this.tcpClient.Client.RemoteEndPoint).Address;

        public bool DataAvailable => this.tcpClient.GetStream().DataAvailable;

        public bool IsConnected => this.tcpClient != null && this.tcpClient.Connected;

        public bool IsLoggedIn { get; set; } = false;

        public bool IsDisconnecting { get; set; } = false;

        public PlayerData Account { get; set; }

        public bool IsEventProtected { get; set; } = false;

        public PlayerClient RtsActionPartner { get; set; }

        public bool InRTSE { get; set; } = false;

        public PlayerClient(TcpClient userSocket)
        {
            this.tcpClient = userSocket;
            this.Account = new PlayerData();
        }

        public void SendData(string data)
        {
            var encryptedData = Encryption.EncryptString(data);
            try
            {
                var sw = new StreamWriter(this.tcpClient.GetStream());
                System.Console.WriteLine(encryptedData);
                sw.WriteLine(encryptedData);
                sw.Flush();
            }
            catch
            {
            }
        }

        public string ReceiveData()
        {
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

        public void Dispose()
        {
            this.tcpClient?.Dispose();
        }
    }
}
