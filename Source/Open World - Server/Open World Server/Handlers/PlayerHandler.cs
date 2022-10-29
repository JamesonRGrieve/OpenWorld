using System;
using System.Collections.Generic;
using System.IO;
using OpenWorldServer.Data;
using OpenWorldServer.Services;
using OpenWorldServer.Utils;

namespace OpenWorldServer.Handlers
{
    internal class PlayerHandler
    {
        private readonly ServerConfig serverConfig;

        // This Props should be private so only the playerhandler handles those lists
        public List<string> WhitelistedUser { get; set; } = new List<string>();

        public List<BannedInfo> BannedPlayers { get; set; } = new List<BannedInfo>();

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
                JsonDataHelper.Save(this.BannedPlayers, PathProvider.BannedPlayersFile);
            }

            this.BannedPlayers = JsonDataHelper.LoadList<BannedInfo>(PathProvider.PlayerWhitelistFile);
            ConsoleUtils.LogToConsole($"Loaded Banlist - {this.BannedPlayers.Count} Entries", ConsoleColor.Green);
        }
    }
}
