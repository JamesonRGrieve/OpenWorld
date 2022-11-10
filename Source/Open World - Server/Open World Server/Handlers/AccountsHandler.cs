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
    public class AccountsHandler
    {
        private readonly ServerConfig serverConfig;

        /// <summary>
        /// Copy of Accounts as ReadOnlyCollection
        /// </summary>
        public ReadOnlyCollection<PlayerData> Accounts => this.accounts.ToList().AsReadOnly();

        private List<PlayerData> accounts = new List<PlayerData>();

        public AccountsHandler(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;
            this.LoadAccounts();
        }

        private void LoadAccounts()
        {
            ConsoleUtils.LogTitleToConsole("Loading Players and Settlements");
            foreach (var playerFile in Directory.GetFiles(PathProvider.PlayersFolderPath, "*.json"))
            {
                this.LoadAccounts(playerFile);
            }

            ConsoleUtils.LogToConsole($"Loaded Players - {this.accounts.Count} Entries", ConsoleUtils.ConsoleLogMode.Info);
        }

        private void LoadAccounts(string playerFile)
        {
            try
            {
                var account = JsonDataHelper.Load<PlayerData>(playerFile);

                if (this.serverConfig.IdleSystem.IsActive)
                {
                    if (account.LastLogin == null)
                    {
                        account.LastLogin = DateTime.Now;
                        this.SaveAccount(account);
                    }

                    var timespan = (DateTime.Now - (DateTime)account.LastLogin);
                    if (timespan.Days > this.serverConfig.IdleSystem.IdleThresholdInDays)
                    {
                        ConsoleUtils.LogToConsole($"Deleting Player [{account.Username}] for Idling ({timespan.Days} Days)", ConsoleUtils.ConsoleLogMode.Warning);
                        File.Delete(playerFile);
                        return;
                    }
                }

                this.accounts.Add(account);
            }
            catch (Exception ex)
            {
                ConsoleUtils.LogToConsole($"Error loading Account from '{playerFile}'. Exception:", ConsoleUtils.ConsoleLogMode.Error);
                ConsoleUtils.LogToConsole(ex.Message, ConsoleUtils.ConsoleLogMode.Error);
            }
        }

        public PlayerData GetAccount(PlayerClient client, bool ignoreLoggedIn = false)
        {
            if (!ignoreLoggedIn && client.IsLoggedIn)
            {
                // No need to access list. If player is already logged in, we mapped the PlayerData object to the Client
                return client.Account;
            }

            return this.GetAccount(client.Account.Username);
        }

        public PlayerData GetAccount(string username)
        {
            var account = this.accounts.Find(pd => pd.Username == username);
            if (account != null && account.Faction != null)
            {
                var factionToGive = Server.savedFactions.Find(fetch => fetch.name == account.Faction.name);
                if (factionToGive != null)
                {
                    account.Faction = factionToGive;
                }
                else
                {
                    account.Faction = null;
                }
            }

            return account;
        }

        public bool SaveAccount(PlayerClient client)
            => this.SaveAccount(client.Account);

        public bool SaveAccount(PlayerData account, bool saveOnly = false)
        {
            var playerFile = this.GetAccountFilePath(account.Username);
            try
            {
                ConsoleUtils.LogToConsole($"Saving [{account.Username}]", ConsoleUtils.ConsoleLogMode.Done);
                JsonDataHelper.Save(account, playerFile);
                if (!saveOnly && !this.Accounts.Contains(account))
                {
                    this.accounts.Add(account);
                    Server.savedClients.Add(new PlayerClient(null) { Account = account });
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ResetAccount(PlayerClient client, bool saveGiftsAndTrades)
            => client.Account = this.ResetAccount(client.Account, saveGiftsAndTrades);

        public PlayerData ResetAccount(PlayerData account, bool saveGiftsAndTrades)
        {
            var newPlayerData = new PlayerData()
            {
                Username = account.Username,
                Password = account.Password,
            };

            if (saveGiftsAndTrades)
            {
                newPlayerData.TradeString = account.TradeString;
                newPlayerData.GiftString = account.GiftString;
            }

            this.RemoveAccount(account);
            this.SaveAccount(newPlayerData);

            return newPlayerData;
        }

        public void RemoveAccount(PlayerData account)
        {
            // We dont use the playerData directly since it could already
            // be a new reference and wouldn't be find in the list by Contains

            var oldData = this.GetAccount(account.Username);
            if (oldData != null)
            {
                this.accounts.Remove(oldData);
            }

            var file = this.GetAccountFilePath(account.Username);
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        internal void ReloadAccounts()
        {
            this.accounts.Clear();
            this.LoadAccounts();
        }

        public PlayerData ReloadAccount(string username)
        {
            var playerFile = this.GetAccountFilePath(username);
            if (File.Exists(playerFile))
            {
                var data = JsonDataHelper.Load<PlayerData>(playerFile);
                var oldData = this.GetAccount(username);
                if (oldData != null)
                {
                    this.accounts.Remove(oldData);
                }

                this.accounts.Add(data);
                return data;
            }

            return null;
        }

        private string GetAccountFilePath(string username) => Path.Combine(PathProvider.PlayersFolderPath, $"{username}.json");
    }
}
