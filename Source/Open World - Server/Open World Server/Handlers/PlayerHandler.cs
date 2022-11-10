using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using OpenWorld.Shared.Networking.Packets;
using OpenWorldServer.Data;

namespace OpenWorldServer.Handlers
{
    public class PlayerHandler
    {
        private readonly ServerConfig serverConfig;

        // This Props should be private so only the playerhandler handles those lists
        public AccountsHandler AccountsHandler { get; }

        public WhitelistHandler WhitelistHandler { get; }

        public BanlistHandler BanlistHandler { get; }

        /// <summary>
        /// Copy of connected clients as ReadOnlyCollection
        /// </summary>
        public ReadOnlyCollection<PlayerClient> ConnectedClients => this.players.ToList().AsReadOnly();

        private List<PlayerClient> players = new List<PlayerClient>();

        public PlayerHandler(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;
            this.AccountsHandler = new AccountsHandler(this.serverConfig);
            this.WhitelistHandler = new WhitelistHandler(this.serverConfig);
            this.BanlistHandler = new BanlistHandler(this.serverConfig);
        }

        internal void AddPlayer(TcpClient newClient)
            => this.players.Add(new PlayerClient(newClient));

        internal void RemovePlayer(PlayerClient client)
        {
            if (this.players.Contains(client))
            {
                if (client.IsLoggedIn)
                {
                    this.AccountsHandler.SaveAccount(client);
                    ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] has Disconnected");
                }
                else
                {
                    ConsoleUtils.LogToConsole("Client [" + client.IPAddress?.ToString() + "] has Disconnected");
                }

                try
                {
                    client.Dispose();
                    this.players.Remove(client);
                }
                catch
                {
                }
            }
        }

        internal void NotifyPlayerListChanged(PlayerClient newClient)
        {
            var clients = this.ConnectedClients;
            var usernames = clients.Select(c => c.Account?.Username).ToArray();
            var packet = new PlayerListPacket(usernames, clients.Count);
            this.SendPacketToAll(clients, packet);
        }

        internal void SendChatMessageToAll(ChatMessagePacket packet)
        {
            string messageForConsole = $"[Chat] {packet.Sender}: {packet.Message}";
            ConsoleUtils.LogToConsole(messageForConsole);
            this.SendPacketToAll(packet);
        }

        public void SendChatMessageToAll(string sender, string message)
            => this.SendChatMessageToAll(new ChatMessagePacket(sender, message));

        internal void SendPacketToAll(IPacket packet) => this.SendPacketToAll(this.ConnectedClients, packet, null);

        internal void SendPacketToAll(ReadOnlyCollection<PlayerClient> clients, IPacket packet) => this.SendPacketToAll(clients, packet, null);

        internal void SendPacketToAll(IPacket packet, PlayerClient clientToSkip) => this.SendPacketToAll(this.ConnectedClients, packet, clientToSkip);

        internal void SendPacketToAll(ReadOnlyCollection<PlayerClient> clients, IPacket packet, PlayerClient clientToSkip)
        {
            if (clientToSkip == null)
            {
                Parallel.ForEach(clients, target => target.SendData(packet));
            }
            else
            {
                Parallel.ForEach(clients, target =>
                {
                    if (clientToSkip?.Account?.Username != target.Account?.Username)
                    {
                        target.SendData(packet);
                    }
                });
            }
        }
    }
}
