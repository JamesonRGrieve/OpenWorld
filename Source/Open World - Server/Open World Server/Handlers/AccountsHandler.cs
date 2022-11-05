using System;
using System.Collections.Generic;
using System.IO;
using OpenWorldServer.Data;
using OpenWorldServer.Services;
using OpenWorldServer.Utils;

namespace OpenWorldServer.Handlers
{
    public class AccountsHandler
    {
        private readonly ServerConfig serverConfig;

        internal List<PlayerData> Accounts { get; set; } = new List<PlayerData>();

        public AccountsHandler(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;
            this.LoadAccounts();
        }

        private void LoadAccounts()
        {
            ConsoleUtils.LogTitleToConsole("Loading Players and Settlements");
            var playerFiles = Directory.GetFiles(PathProvider.PlayersFolderPath, "*.json");

            foreach (var playerFile in playerFiles)
            {
                this.Accounts.Add(JsonDataHelper.Load<PlayerData>(playerFile));
            }

            ConsoleUtils.LogToConsole($"Loaded Players - {playerFiles.Length} Entries", ConsoleColor.Green);
        }

        public PlayerData GetAccount(PlayerClient client)
        {
            if (client.IsLoggedIn)
            {
                // No need to access list. If player is already logged in, we mapped the PlayerData object to the Client
                return client.Account;
            }

            return this.GetAccount(client.Account.Username);
        }

        public PlayerData GetAccount(string username)
            => this.Accounts.Find(pd => pd.Username == username);

        public bool SaveAccount(PlayerClient client)
            => this.SaveAccount(client.Account);

        public bool SaveAccount(PlayerData account)
        {
            var playerFile = this.GetAccountFromFilePath(account.Username);
            try
            {
                ConsoleUtils.LogToConsole($"Saving [{account.Username}]", ConsoleColor.DarkGreen);
                JsonDataHelper.Save(account, playerFile);
                if (!this.Accounts.Contains(account))
                {
                    this.Accounts.Add(account);
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
                this.Accounts.Remove(oldData);
            }

            var file = this.GetAccountFromFilePath(account.Username);
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        public PlayerData ReloadAccount(string username)
        {
            var playerFile = this.GetAccountFromFilePath(username);
            if (File.Exists(playerFile))
            {
                var data = JsonDataHelper.Load<PlayerData>(playerFile);
                var oldData = this.GetAccount(username);
                if (oldData != null)
                {
                    this.Accounts.Remove(oldData);
                }

                this.Accounts.Add(data);
                return data;
            }

            return null;
        }

        private string GetAccountFromFilePath(string username) => Path.Combine(PathProvider.PlayersFolderPath, $"{username}.json");
    }
}
