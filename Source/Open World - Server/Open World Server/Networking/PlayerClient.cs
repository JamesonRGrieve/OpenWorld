using System.Net;
using System.Net.Sockets;
using OpenWorldServer.Data;

namespace OpenWorldServer
{
    public class PlayerClient
    {
        private readonly TcpClient tcpClient;

        public IPAddress IPAddress => ((IPEndPoint)this.tcpClient.Client.RemoteEndPoint).Address;

        public NetworkStream ClientStream => this.tcpClient.GetStream();

        public bool IsConnected => this.tcpClient != null && this.tcpClient.Connected;

        public bool IsLoggedIn { get; set; } = false;

        public bool IsDisconnecting { get; set; } = false;

        public PlayerData PlayerData { get; set; }

        public bool IsEventProtected { get; set; } = false;

        public PlayerClient RtsActionPartner { get; set; }

        public bool InRTSE { get; set; } = false;

        public PlayerClient(TcpClient userSocket)
        {
            this.tcpClient = userSocket;
            this.PlayerData = new PlayerData();
        }

        public void Dispose()
        {
            this.tcpClient?.Dispose();
        }
    }
}
