using System.IO;
using OpenWorldServer.Data;
using OpenWorldServer.Services;
using OpenWorldServer.Utils;

namespace OpenWorldServer.Handlers
{
    internal class PlayerHandler
    {
        private readonly ServerConfig serverConfig;

        public PlayerWhitelist PlayerWhitelist { get; private set; }

        public PlayerHandler(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;

            ConsoleUtils.LogToConsole($"Whiteliste Mode: {this.serverConfig.WhitelistMode}");
            this.ReloadWhitelist();
        }

        private void ReloadWhitelist()
        {
            ConsoleUtils.LogTitleToConsole("Reloading Whitelist");
            if (!File.Exists(PathProvider.PlayerWhitelistFile))
            {
                this.PlayerWhitelist = new PlayerWhitelist();
                ConsoleUtils.LogToConsole($"Generating new Whitelist file..", System.ConsoleColor.Yellow);
                JsonDataHelper.Save(this.PlayerWhitelist, PathProvider.PlayerWhitelistFile);
            }

            this.PlayerWhitelist = JsonDataHelper.Load<PlayerWhitelist>(PathProvider.PlayerWhitelistFile);
            ConsoleUtils.LogToConsole("Loaded whitelist", System.ConsoleColor.Green);
        }
    }
}
