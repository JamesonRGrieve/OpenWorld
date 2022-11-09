using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using OpenWorld.Shared.Networking;
using OpenWorld.Shared.Networking.Packets;
using OpenWorldServer.Data;
using OpenWorldServer.Enums;
using OpenWorldServer.Handlers.Old;

namespace OpenWorldServer.Handlers
{
    internal class ConnectionHandler
    {
        private readonly ServerConfig serverConfig;
        private readonly PlayerHandler playerHandler;
        private readonly ModHandler modHandler;
        private readonly WorldMapHandler worldMapHandler;

        public ConnectionHandler(ServerConfig serverConfig, PlayerHandler playerHandler, ModHandler modHandler, WorldMapHandler worldMapHandler)
        {
            this.serverConfig = serverConfig;
            this.playerHandler = playerHandler;
            this.modHandler = modHandler;
            this.worldMapHandler = worldMapHandler;
        }

        public void ReadDataFromClient(PlayerClient client)
        {
            string data = null;
            try
            {
                data = client.ReceiveData();
            }
            catch { }

            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            if (data.StartsWith("Connect│"))
            {
                this.HandleNewConnection(client, PacketHandler.GetPacket<ConnectPacket>(data));
            }
            else if (data.StartsWith("ChatMessage│"))
            {
                NetworkingHandler.ChatMessageHandle(client, data);
            }
            else if (data.StartsWith("UserSettlement│"))
            {
                NetworkingHandler.UserSettlementHandle(client, data);
            }
            else if (data.StartsWith("ForceEvent│"))
            {
                NetworkingHandler.ForceEventHandle(client, data);
            }
            else if (data.StartsWith("SendGiftTo│"))
            {
                NetworkingHandler.SendGiftHandle(client, data);
            }
            else if (data.StartsWith("SendTradeTo│"))
            {
                NetworkingHandler.SendTradeHandle(client, data);
            }
            else if (data.StartsWith("SendBarterTo│"))
            {
                NetworkingHandler.SendBarterHandle(client, data);
            }
            else if (data.StartsWith("TradeStatus│"))
            {
                NetworkingHandler.TradeStatusHandle(client, data);
            }
            else if (data.StartsWith("BarterStatus│"))
            {
                NetworkingHandler.BarterStatusHandle(client, data);
            }
            else if (data.StartsWith("GetSpyInfo│"))
            {
                NetworkingHandler.SpyInfoHandle(client, data);
            }
            else if (data.StartsWith("FactionManagement│"))
            {
                NetworkingHandler.FactionManagementHandle(client, data);
            }
        }

        private void HandleNewConnection(PlayerClient client, ConnectPacket packet)
        {
            if (!this.IsValidUsername(packet.Username))
            {
                Networking.SendData(client, "Disconnect│Corrupted");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole($"Player [{client.IPAddress?.ToString()}] tried to Join but had an invalid Username [{packet.Username ?? ""}]");
                return;
            }

            if (this.IsBanned(client.IPAddress)) // We check here the ip, so we don't need to more checks if client is banned
            {
                Networking.SendData(client, "Disconnect│Banned");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole($"Player [{packet.Username}] tried to Join but is Banned (IP)");
                return;
            }

            if (!this.IsClientVersionValid(packet.Version))
            {
                Networking.SendData(client, "Disconnect│Version");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole($"Player [{packet.Username}] tried to Join but is using wrong Version [{packet.Version}]");
                return;
            }

            if (this.IsBanned(packet.Username)) // Since everything is valid, check if username is banned
            {
                Networking.SendData(client, "Disconnect│Banned");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole($"Player [{packet.Username}] tried to Join but is Banned (Username)");
                return;
            }

            if (this.IsUserAlreadyConnected(packet.Username))
            {
                Networking.SendData(client, "Disconnect│AnotherLogin");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole($"Player [{packet.Username}] tried to Join but is already LoggedIn / Connected");
                return;
            }

            var account = this.playerHandler.AccountsHandler.GetAccount(packet.Username);
            if (account != null)
            {
                if (account.Password != client.Account.Password)
                {
                    Networking.SendData(client, "Disconnect│WrongPassword");
                    client.IsDisconnecting = true;
                    ConsoleUtils.LogToConsole($"Player [{packet.Username}] tried to Join but entered an invalid password");
                    return;
                }

                client.Account = account;
                client.IsLoggedIn = true;
                ConsoleUtils.LogToConsole($"Player [{client.Account.Username}] has logged in");
            }
            else
            {
                client.Account.Username = packet.Username;
                client.Account.Password = packet.Password;
                this.playerHandler.AccountsHandler.SaveAccount(client);
                ConsoleUtils.LogToConsole("New Player [" + client.Account.Username + "] has logged in");
            }

            if (!client.Account.IsAdmin && this.IsServerFull())
            {
                Networking.SendData(client, "Disconnect│ServerFull");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole($"Player [{client.Account.Username}] tried to Join but Server is full");
                return;
            }

            if (!this.playerHandler.WhitelistHandler.IsWhitelisted(client.Account.Username))
            {
                Networking.SendData(client, "Disconnect│Whitelist");
                client.IsDisconnecting = true;
                ConsoleUtils.LogToConsole($"Player [{client.Account.Username}] tried to Join but is not Whitelisted");
                return;
            }

            if (this.serverConfig.ModsSystem.MatchModlist &&
                !client.Account.IsAdmin)
            {
                var flaggedMods = this.AreClientModsValid(packet.Mods);
                if (flaggedMods != null)
                {
                    const string modSeperator = "»";
                    var invalidMods = string.Join(modSeperator, flaggedMods);
                    Networking.SendData(client, "Disconnect│WrongMods│" + invalidMods);
                    client.IsDisconnecting = true;
                    ConsoleUtils.LogToConsole($"Player [{packet.Username}] has a Mod File Mismatch ({flaggedMods.Count} Mods mismatch)");
                    return;
                }
            }

            ConsoleUtils.UpdateTitle();
            JoinPlayer(client, packet.JoinMode);
            ServerUtils.SendPlayerListToAll(client);
        }

        private bool IsServerFull()
            => this.playerHandler.ConnectedClients.Count >= StaticProxy.serverConfig.MaxPlayers;

        private bool IsClientVersionValid(string clientVersion)
            => string.IsNullOrWhiteSpace(Server.latestClientVersion) || clientVersion == Server.latestClientVersion;

        private bool IsValidUsername(string username)
            => !string.IsNullOrWhiteSpace(username) && username.All(character => char.IsLetterOrDigit(character) || character == '_' || character == '-');

        private bool IsBanned(string username) => this.playerHandler.BanlistHandler.GetBanInfo(username) != null;

        private bool IsBanned(IPAddress ip) => (this.playerHandler.BanlistHandler.GetBanInfo(ip)?.Length ?? 0) != 0;

        private bool IsUserAlreadyConnected(string username)
            => this.playerHandler.ConnectedClients.Any(c => username == c.Account.Username);

        private List<string> AreClientModsValid(string[] mods)
        {
            var flaggedMods = new List<string>();

            foreach (string mod in mods)
            {
                if (this.modHandler.IsModWhitelisted(mod)) // Mod is optional allowed
                {
                    // We do nothing~
                }
                else if (this.modHandler.IsModBlacklisted(mod)) // Mod is forbidden
                {
                    flaggedMods.Add(mod);
                }
                else if (!this.modHandler.IsModEnforced(mod)) // Mod is neither whitelisted nor a required mod (Treat it as an unallowed mod)
                {
                    flaggedMods.Add(mod);
                }
            }

            foreach (var modMetaData in this.modHandler.RequiredMods)
            {
                if (!mods.Contains(modMetaData.Name))
                {
                    flaggedMods.Add(modMetaData.Name);
                }
            }

            return flaggedMods;
        }

        private void JoinPlayer(PlayerClient client, JoinMode joinMode)
        {
            if (joinMode == JoinMode.NewGame)
            {
                ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] started a new game");
                this.SendNewGameData(client);
            }
            else if (joinMode == JoinMode.LoadGame)
            {
                this.SendLoadGameData(client);
            }

            ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] joined");
        }

        private void SendWorldData(PlayerClient client)
        {
            Networking.SendData(client, JoiningsUtils.GetSettlementsToSend(client));
            Networking.SendData(client, JoiningsUtils.GetVariablesToSend(client));

            var usernames = this.playerHandler.ConnectedClients.Select(c => c.Account?.Username);
            client.SendData(new PlayerListPacket(usernames.ToArray()));

            Networking.SendData(client, FactionHandler.GetFactionDetails(client));
            Networking.SendData(client, FactionBuildingHandler.GetAllFactionStructures(client));
        }

        private void SendNewGameData(PlayerClient client)
        {
            this.worldMapHandler.NotifySettlementRemoved(client);
            this.playerHandler.AccountsHandler.ResetAccount(client, true);

            Networking.SendData(client, JoiningsUtils.GetPlanetToSend());

            this.SendWorldData(client);

            Networking.SendData(client, "NewGame│");
        }

        private void SendLoadGameData(PlayerClient client)
        {
            this.SendWorldData(client);

            Networking.SendData(client, JoiningsUtils.GetGiftsToSend(client));
            Networking.SendData(client, JoiningsUtils.GetTradesToSend(client));
            Networking.SendData(client, "LoadGame│");
        }
    }
}
