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
        public ReadOnlyCollection<Account> Accounts => this.accounts.ToList().AsReadOnly();

        private List<Account> accounts = new List<Account>();

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
                var account = JsonDataHelper.Load<Account>(playerFile);

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

        public Account GetAccount(Guid id)
        {
            var account = this.Accounts.FirstOrDefault(a => a.Id == id);
            this.SetFaction(account);

            return account;
        }

        public Account GetAccount(string username)
        {
            var account = this.Accounts.FirstOrDefault(a => a.Username == username);
            this.SetFaction(account);

            return account;
        }

        private void SetFaction(Account account)
        {
            if (account == null || account.Faction == null)
            {
                return;
            }

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

        public bool SaveAccount(PlayerClient client)
            => this.SaveAccount(client.Account);

        public bool SaveAccount(Account account, bool saveOnly = false)
        {
            try
            {
                ConsoleUtils.LogToConsole($"Saving Account [{account.Username}]", ConsoleUtils.ConsoleLogMode.Done);
                JsonDataHelper.Save(account, this.GetAccountFilePath(account.Username));
                var knownAccount = this.Accounts.FirstOrDefault(a => a.Id == account.Id);
                if (!saveOnly && knownAccount == null)
                {
                    this.accounts.Add(account);
                }

                return true;
            }
            catch (Exception ex)
            {
                ConsoleUtils.LogToConsole($"Error saving Account [{account.Username}]", ConsoleUtils.ConsoleLogMode.Error);
                ConsoleUtils.LogToConsole(ex.Message, ConsoleUtils.ConsoleLogMode.Error);
                return false;
            }
        }

        public void ResetAccount(PlayerClient client, bool saveGiftsAndTrades)
            => client.Account = this.ResetAccount(client.Account, saveGiftsAndTrades);

        public Account ResetAccount(Account account, bool saveGiftsAndTrades)
        {
            var newAccount = new Account()
            {
                Id = account.Id,
                Username = account.Username,
                Password = account.Password,
            };

            if (saveGiftsAndTrades)
            {
                newAccount.TradeString = account.TradeString;
                newAccount.GiftString = account.GiftString;
            }

            this.RemoveAccount(account);
            this.SaveAccount(newAccount);

            return newAccount;
        }

        public void RemoveAccount(Account account)
        {
            // We dont use the Account directly since it could already
            // be a new reference and wouldn't be find in the list by Contains

            var oldData = this.GetAccount(account.Id);
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

        public Account ReloadAccount(string username)
        {
            var playerFile = this.GetAccountFilePath(username);
            if (File.Exists(playerFile))
            {
                var data = JsonDataHelper.Load<Account>(playerFile);
                var oldData = this.GetAccount(data.Id);
                if (oldData != null)
                {
                    this.accounts.Remove(oldData);
                }

                this.accounts.Add(data);
                return data;
            }

            return null;
        }

        private string SanitizedName(string name) => name.Replace(' ', '-');

        private string GetAccountFilePath(string username) => Path.Combine(PathProvider.PlayersFolderPath, $"{this.SanitizedName(username)}.json");
    }
}
