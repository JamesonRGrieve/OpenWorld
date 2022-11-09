using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
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

        public ReadOnlyCollection<PlayerClient> ConnectedClients => this.players.AsReadOnly();

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
    }
}
