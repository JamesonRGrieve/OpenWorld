using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using OpenWorldServer.Data;
using OpenWorldServer.Services;
using OpenWorldServer.Utils;

namespace OpenWorldServer.Handlers
{
    public class PlayerHandler
    {
        private readonly ServerConfig serverConfig;

        // This Props should be private so only the playerhandler handles those lists
        public List<string> WhitelistedUser { get; set; } = new List<string>();

        public List<BanInfo> BannedPlayers { get; set; } = new List<BanInfo>();

        public List<PlayerData> PlayerData { get; set; } = new List<PlayerData>();

        public PlayerHandler(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;

            ConsoleUtils.LogToConsole($"Whitelist Mode: {this.serverConfig.WhitelistMode}");
            this.ReloadWhitelist();
            this.ReloadBannedPlayers();
            this.LoadPlayerData();
        }

        private void ReloadWhitelist()
        {
            ConsoleUtils.LogTitleToConsole("Reloading Whitelist");
            if (!File.Exists(PathProvider.PlayerWhitelistFile))
            {
                ConsoleUtils.LogToConsole($"Generating new Whitelist file..", ConsoleColor.Yellow);
                JsonDataHelper.Save(this.WhitelistedUser, PathProvider.PlayerWhitelistFile);
            }

            this.WhitelistedUser = JsonDataHelper.LoadList<string>(PathProvider.PlayerWhitelistFile);
            ConsoleUtils.LogToConsole($"Loaded whitelist - {this.WhitelistedUser.Count} Entries", ConsoleColor.Green);
        }

        internal bool IsWhitelisted(ServerClient client)
            => client.PlayerData.IsAdmin || this.IsWhitelisted(client.PlayerData.Username);

        private bool IsWhitelisted(string username)
            => !this.serverConfig.WhitelistMode || this.WhitelistedUser.Contains(username);

        private void ReloadBannedPlayers()
        {
            ConsoleUtils.LogTitleToConsole("Reloading Banned Players");
            if (!File.Exists(PathProvider.BannedPlayersFile))
            {
                ConsoleUtils.LogToConsole($"Generating new Banlist file..", ConsoleColor.Yellow);
                this.SaveBannedPlayers();
            }

            this.BannedPlayers = JsonDataHelper.LoadList<BanInfo>(PathProvider.PlayerWhitelistFile);
            ConsoleUtils.LogToConsole($"Loaded Banlist - {this.BannedPlayers.Count} Entries", ConsoleColor.Green);
        }

        private void SaveBannedPlayers() => JsonDataHelper.Save(this.BannedPlayers, PathProvider.BannedPlayersFile);

        internal void BanPlayer(ServerClient client, string reason = "")
        {
            var ip = ((IPEndPoint)client.tcp.Client.RemoteEndPoint).Address.ToString();
            client.disconnectFlag = true;
            this.BannedPlayers.Add(new BanInfo()
            {
                Username = client.PlayerData.Username,
                IPAddress = ip,
                BanDate = DateTime.Now,
                Reason = reason,
            });

            this.SaveBannedPlayers();
            ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] has been Banned", ConsoleColor.Green);
        }

        internal BanInfo GetBanInfo(string username) => this.BannedPlayers.Find(b => b.Username == username);

        internal void UnbanPlayer(string username)
        {
            var banInfo = this.GetBanInfo(username);
            if (banInfo == null)
            {
                ConsoleUtils.LogToConsole("Player [" + username + "] is not banned", ConsoleColor.Yellow);
            }

            this.UnbanPlayer(banInfo);
        }

        internal void UnbanPlayer(BanInfo banInfo)
        {
            this.BannedPlayers.Remove(banInfo);
            this.SaveBannedPlayers();
            ConsoleUtils.LogToConsole("Player [" + banInfo.Username + "] has been Unbanned", ConsoleColor.Green);
        }

        private void LoadPlayerData()
        {
            ConsoleUtils.LogTitleToConsole("Loading Players and Settlements");
            var playerFiles = Directory.GetFiles(PathProvider.PlayersFolderPath, "*.json");

            foreach (var playerFile in playerFiles)
            {
                this.PlayerData.Add(JsonDataHelper.Load<PlayerData>(playerFile));
            }

            ConsoleUtils.LogToConsole($"Loaded Players - {playerFiles.Length} Entries", ConsoleColor.Green);
        }

        public PlayerData GetPlayerData(ServerClient client)
        {
            if (client.IsLoggedIn)
            {
                // No need to access list. If player is already logged in, we mapped the PlayerData object to the Client
                return client.PlayerData;
            }

            return this.GetPlayerData(client.PlayerData.Username);
        }

        public PlayerData GetPlayerData(string username)
            => this.PlayerData.Find(pd => pd.Username == username);

        public bool SavePlayerData(ServerClient client)
            => this.SavePlayerData(client.PlayerData);

        public bool SavePlayerData(PlayerData playerData)
        {
            var playerFile = this.GetPlayerDataFilePath(playerData.Username);
            try
            {
                JsonDataHelper.Save(playerData, playerFile);
                if (!this.PlayerData.Contains(playerData))
                {
                    this.PlayerData.Add(playerData);
                    Server.savedClients.Add(new ServerClient(null) { PlayerData = playerData });
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ResetPlayerData(ServerClient client, bool saveGiftsAndTrades)
            => client.PlayerData = this.ResetPlayerData(client.PlayerData, saveGiftsAndTrades);

        public PlayerData ResetPlayerData(PlayerData playerData, bool saveGiftsAndTrades)
        {
            var newPlayerData = new PlayerData()
            {
                Username = playerData.Username,
                Password = playerData.Password,
            };

            // ToDo: Cleanup Settlement BUT NOT LIKE THIS
            WorldUtils.RemoveSettlement(null, null);

            if (saveGiftsAndTrades)
            {
                newPlayerData.TradeString = playerData.TradeString;
                newPlayerData.GiftString = playerData.GiftString;
            }

            this.RemovePlayData(playerData);
            this.SavePlayerData(newPlayerData);

            return newPlayerData;
        }

        public void RemovePlayData(PlayerData playerData)
        {
            // We dont use the playerData directly since it could already
            // be a new reference and wouldn't be find in the list by Contains

            var oldData = this.GetPlayerData(playerData.Username);
            if (oldData != null)
            {
                this.PlayerData.Remove(oldData);
            }

            var file = this.GetPlayerDataFilePath(playerData.Username);
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        public PlayerData ReloadPlayerData(string username)
        {
            var playerFile = this.GetPlayerDataFilePath(username);
            if (File.Exists(playerFile))
            {
                var data = JsonDataHelper.Load<PlayerData>(playerFile);
                var oldData = this.GetPlayerData(username);
                if (oldData != null)
                {
                    this.PlayerData.Remove(oldData);
                }

                this.PlayerData.Add(data);
                return data;
            }

            return null;
        }

        private string GetPlayerDataFilePath(string username) => Path.Combine(PathProvider.PlayersFolderPath, $"{username}.json");
    }
}
