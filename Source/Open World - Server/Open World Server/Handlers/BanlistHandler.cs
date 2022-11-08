using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OpenWorldServer.Data;
using OpenWorldServer.Services;
using OpenWorldServer.Utils;

namespace OpenWorldServer.Handlers
{
    public class BanlistHandler
    {
        private readonly ServerConfig serverConfig;

        public ReadOnlyCollection<BanInfo> Banlist => this.banlist.AsReadOnly();

        private List<BanInfo> banlist = new List<BanInfo>();

        public BanlistHandler(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;
            this.ReloadBannedPlayers();
        }

        private void ReloadBannedPlayers()
        {
            ConsoleUtils.LogTitleToConsole("Reloading Banned Players");
            if (!File.Exists(PathProvider.BannedPlayersFile))
            {
                ConsoleUtils.LogToConsole($"Generating new Banlist file..", ConsoleUtils.ConsoleLogMode.Warning);
                this.SaveBannedPlayers();
            }

            this.banlist = JsonDataHelper.LoadList<BanInfo>(PathProvider.BannedPlayersFile);
            ConsoleUtils.LogToConsole($"Loaded Banlist - {this.banlist.Count} Entries", ConsoleUtils.ConsoleLogMode.Info);
        }

        private void SaveBannedPlayers() => JsonDataHelper.Save(this.banlist.ToList(), PathProvider.BannedPlayersFile);

        internal void BanPlayer(PlayerClient client, string reason = "")
        {
            var ip = client.IPAddress.ToString();
            client.IsDisconnecting = true;
            this.banlist.Add(new BanInfo()
            {
                Username = client.Account.Username,
                IPAddress = ip,
                BanDate = DateTime.Now,
                Reason = reason,
            });

            this.SaveBannedPlayers();
            ConsoleUtils.LogToConsole("Player [" + client.Account.Username + "] has been Banned", ConsoleUtils.ConsoleLogMode.Info);
        }

        internal BanInfo GetBanInfo(string username) => this.banlist.Find(b => b.Username == username);

        internal void UnbanPlayer(string username)
        {
            var banInfo = this.GetBanInfo(username);
            if (banInfo == null)
            {
                ConsoleUtils.LogToConsole("Player [" + username + "] is not banned", ConsoleUtils.ConsoleLogMode.Warning);
            }

            this.UnbanPlayer(banInfo);
        }

        internal void UnbanPlayer(BanInfo banInfo)
        {
            this.banlist.Remove(banInfo);
            this.SaveBannedPlayers();
            ConsoleUtils.LogToConsole("Player [" + banInfo.Username + "] has been Unbanned", ConsoleUtils.ConsoleLogMode.Info);
        }
    }
}
