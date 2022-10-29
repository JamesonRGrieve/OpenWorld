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

        public PlayerHandler(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;

            ConsoleUtils.LogToConsole($"Whitelist Mode: {this.serverConfig.WhitelistMode}");
            this.ReloadWhitelist();
            this.ReloadBannedPlayers();
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
            ConsoleUtils.LogToConsole("Player [" + client.PlayerData.Username + "] Has Been Banned", ConsoleColor.Green);
        }

        internal void UnbanPlayer(string username)
        {
            var banInfo = this.BannedPlayers.Find(b => b.Username == username);
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
    }
}
