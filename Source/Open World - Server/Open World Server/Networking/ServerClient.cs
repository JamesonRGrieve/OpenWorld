using System;
using System.Net.Sockets;
using OpenWorldServer.Data;

namespace OpenWorldServer
{
    [System.Serializable]
    public class ServerClient
    {
        public bool IsLoggedIn { get; set; } = false;

        public PlayerData PlayerData { get; set; }

        [NonSerialized] public TcpClient tcp;
        [NonSerialized] public bool disconnectFlag;
        [NonSerialized] public bool eventShielded;
        [NonSerialized] public bool inRTSE;
        [NonSerialized] public ServerClient inRtsActionWith;

        public ServerClient(TcpClient userSocket)
        {
            tcp = userSocket;
            this.PlayerData = new PlayerData();
        }
    }
}
