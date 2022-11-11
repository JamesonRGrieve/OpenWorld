using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using OpenWorld.Shared.Networking.Packets;
using OpenWorldServer.Data;
using OpenWorldServer.Handlers;

namespace OpenWorldServer.Manager
{
    public class PlayerManager
    {
        private readonly ServerConfig serverConfig;

        // This Props should be private so only the playerhandler handles those lists
        public AccountsHandler AccountsHandler { get; }

        public WhitelistHandler WhitelistHandler { get; }

        public BanlistHandler BanlistHandler { get; }

        /// <summary>
        /// Copy of connected clients as ReadOnlyCollection
        /// </summary>
        public ReadOnlyCollection<PlayerClient> ConnectedClients => players.ToList().AsReadOnly();

        private List<PlayerClient> players = new List<PlayerClient>();

        public PlayerManager(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;
            AccountsHandler = new AccountsHandler(this.serverConfig);
            WhitelistHandler = new WhitelistHandler(this.serverConfig);
            BanlistHandler = new BanlistHandler(this.serverConfig);
        }

        internal PlayerClient GetClient(Guid accountId)
            => this.ConnectedClients.FirstOrDefault(c => c.Account?.Id == accountId);

        internal void AddPlayer(TcpClient newClient)
            => players.Add(new PlayerClient(newClient));

        internal void RemovePlayer(PlayerClient client)
        {
            if (players.Contains(client))
            {
                if (client.IsLoggedIn)
                {
                    AccountsHandler.SaveAccount(client);
                    ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] has Disconnected");
                }
                else
                {
                    ConsoleUtils.LogToConsole("Client [" + client.IPAddress?.ToString() + "] has Disconnected");
                }

                try
                {
                    client.Dispose();
                    players.Remove(client);
                }
                catch
                {
                }
            }
        }

        internal void NotifyPlayerListChanged(PlayerClient newClient)
        {
            var clients = ConnectedClients;
            var usernames = clients.Select(c => c.Account?.Username).ToArray();
            var packet = new PlayerListPacket(usernames, clients.Count);
            SendPacketToAll(clients, packet);
        }

        internal void SendChatMessageToAll(ChatMessagePacket packet)
        {
            string messageForConsole = $"[Chat] {packet.Sender}: {packet.Message}";
            ConsoleUtils.LogToConsole(messageForConsole);
            SendPacketToAll(packet);
        }

        public void SendChatMessageToAll(string sender, string message)
            => SendChatMessageToAll(new ChatMessagePacket(sender, message));

        internal void SendPacketToAll(IPacket packet) => SendPacketToAll(ConnectedClients, packet, null);

        internal void SendPacketToAll(ReadOnlyCollection<PlayerClient> clients, IPacket packet) => SendPacketToAll(clients, packet, null);

        internal void SendPacketToAll(IPacket packet, PlayerClient clientToSkip) => SendPacketToAll(ConnectedClients, packet, clientToSkip);

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
                    if (clientToSkip != null && clientToSkip.Account?.Id != target.Account?.Id)
                    {
                        target.SendData(packet);
                    }
                });
            }
        }
    }
}
