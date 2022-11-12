using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using OpenWorldServer.Data;
using OpenWorldServer.Services;
using OpenWorldServer.Utils;

namespace OpenWorldServer.Handlers
{
    public class WhitelistHandler
    {
        private readonly ServerConfig serverConfig;

        public ReadOnlyCollection<string> Whitelist => this.whitelist.AsReadOnly();

        private List<string> whitelist = new List<string>();

        public WhitelistHandler(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;

            ConsoleUtils.LogToConsole($"Whitelist Mode: {this.serverConfig.WhitelistMode}");
            this.ReloadWhitelist();
        }

        private void ReloadWhitelist()
        {
            ConsoleUtils.LogTitleToConsole("Reloading Whitelist");
            if (!File.Exists(PathProvider.PlayerWhitelistFile))
            {
                ConsoleUtils.LogToConsole($"Generating new Whitelist file..", ConsoleUtils.ConsoleLogMode.Info);
                JsonDataHelper.Save(this.whitelist, PathProvider.PlayerWhitelistFile);
            }

            this.whitelist = JsonDataHelper.LoadList<string>(PathProvider.PlayerWhitelistFile);
            ConsoleUtils.LogToConsole($"Loaded whitelist - {this.whitelist.Count} Entries", ConsoleUtils.ConsoleLogMode.Info);
        }

        internal bool IsWhitelisted(PlayerClient client)
            => client.Account.IsAdmin || this.IsWhitelisted(client.Account.Username);

        internal bool IsWhitelisted(string username)
            => !this.serverConfig.WhitelistMode || this.whitelist.Contains(username);
    }
}
