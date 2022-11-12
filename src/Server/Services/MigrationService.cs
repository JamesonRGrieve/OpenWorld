using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using OpenWorld.Server.Data;
using OpenWorld.Server.Deprecated;
using OpenWorld.Server.Utils;
using OpenWorld.Server.Data.Factions;
using static OpenWorld.Server.Handlers.Old.FactionHandler;
using OpenWorld.Server.Handlers.Old;

namespace OpenWorld.Server.Services
{
    internal class MigrationService
    {
        private string fileBackupPath = Path.Combine(PathProvider.MainFolderPath, "MigrationBackups");

        public void MigrateAll(ServerConfig serverConfig)
        {
            ConsoleUtils.LogTitleToConsole("Checking for Migrations");
            this.MigrateConfig(serverConfig);
            this.MigrateWorldSettings(serverConfig);
            JsonDataHelper.Save(serverConfig, PathProvider.ConfigFile);
            this.MigrateWhitelist();
            this.MigrateBanlist();
            this.MigrateModsFolder();
            MigrateFactions(MigratePlayers());
        }

        private List<Account> MigratePlayers()
        {
            List<Account> migratedPlayers = new List<Account>();
            var oldPath = Path.Combine(PathProvider.MainFolderPath, "Players");
            if (!Directory.Exists(oldPath)) ConsoleUtils.LogToConsole($"No old players folder detected, skipping.");
            else
            {
                string[] playerFiles = Directory.GetFiles(oldPath);
                int failedToLoadPlayers = 0;
                

                foreach (string player in playerFiles)
                {
                    try
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        FileStream s = File.Open(player, FileMode.Open);
                        object obj = formatter.Deserialize(s);
                        ServerClient loaded = (ServerClient)obj;
                        

                        s.Flush();
                        s.Close();
                        s.Dispose();

                        Account imported = new Account()
                        { 
                            Username = loaded.username,
                            Password = loaded.password,
                            IsAdmin =  loaded.isAdmin,
                            ToWipe = loaded.toWipe,
                            HomeTileId = loaded.homeTileID,
                            GiftString = loaded.giftString,
                            TradeString = loaded.tradeString,
                            Faction = loaded.faction,
                            PawnCount = loaded.pawnCount,
                            Wealth = loaded.wealth,
                            IsImmunized = loaded.isImmunized
                        };
                        migratedPlayers.Add(imported);
                        JsonDataHelper.Save(imported, Path.Combine(PathProvider.PlayersFolderPath, $"{imported.Username}.json"));
                    }
                    catch { failedToLoadPlayers++; }
                }
                

                ConsoleUtils.LogToConsole($"Imported {migratedPlayers.Count} Old Factions");

                if (failedToLoadPlayers > 0) ConsoleUtils.LogToConsole($"Failed to load {failedToLoadPlayers} Players", ConsoleUtils.ConsoleLogMode.Error);
                
            }
            return migratedPlayers;
        }
        private void MigrateFactions(List<Account> migratedPlayers)
        {
            var oldPath = Path.Combine(PathProvider.MainFolderPath, "Factions");
            if (!Directory.Exists(oldPath)) ConsoleUtils.LogToConsole($"No old factions folder detected, skipping.");
            else
            {
                string[] factionFiles = Directory.GetFiles(oldPath);
                int failedToLoadFactions = 0, importedFactions = 0;

                foreach (string faction in factionFiles)
                {
                    try
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        FileStream s = File.Open(faction, FileMode.Open);
                        object obj = formatter.Deserialize(s);
                        FactionOld factionToLoad = (FactionOld)obj;

                        s.Flush();
                        s.Close();
                        s.Dispose();

                        Faction imported = new Faction()
                        {
                            Name = factionToLoad.name,
                            Wealth = factionToLoad.wealth,
                            Structures = new List<object>(factionToLoad.factionStructures)
                        };
                        // TODO: Patch out the necessity to reference MemberRank here.
                        foreach (KeyValuePair<PlayerClient, MemberRank> oldFactionMember in factionToLoad.members) imported.Members.Add(migratedPlayers.Find(x => x.Username==oldFactionMember.Key.Account.Username).Id, (Shared.Enums.FactionRank)(byte)oldFactionMember.Value);

                        JsonDataHelper.Save(imported, Path.Combine(PathProvider.FactionsFolderPath, $"{imported.Name}.json"));
                    }
                    catch { failedToLoadFactions++; }
                }

                ConsoleUtils.LogToConsole($"Imported {importedFactions} Old Factions");

                if (failedToLoadFactions > 0) ConsoleUtils.LogToConsole($"Failed to load {failedToLoadFactions} Factions", ConsoleUtils.ConsoleLogMode.Error);
            }
        }

        public static MigrationService CreateAndMigrateAll(ServerConfig serverConfig)
        {
            var service = new MigrationService();
            service.MigrateAll(serverConfig);

            return service;
        }

        private void EnsureBackupDir()
            => Directory.CreateDirectory(this.fileBackupPath);

        private void LogMigratingData(string data)
            => ConsoleUtils.LogToConsole($"Migrating {data}", ConsoleUtils.ConsoleLogMode.Warning);

        private void LogMigratedData(string data, bool successfully, string reason = "")
        {
            var state = successfully ? "successfully" : "failed";
            var color = successfully ? ConsoleUtils.ConsoleLogMode.Info : ConsoleUtils.ConsoleLogMode.Error;
            reason = successfully ? string.Empty : $" ({reason})";
            ConsoleUtils.LogToConsole($"Migrated {data} {state}{reason}", color);
        }

        public void MigrateConfig(ServerConfig serverConfig)
        {
            var oldPath = Path.Combine(PathProvider.MainFolderPath, "Server Settings.txt");
            if (File.Exists(oldPath))
            {
                const string migrating = "Server Settings";
                this.EnsureBackupDir();
                this.LogMigratingData(migrating);

                var settings = File.ReadAllLines(oldPath);

                const string namePrefix = "Server Name: ";
                const string descriptionPrefix = "Server Description: ";
                const string localIpPrefix = "Server Local IP: ";
                const string portPrefix = "Server Port: ";
                const string maxPlayerPrefix = "Max Players: ";
                const string devModePrefix = "Allow Dev Mode: ";
                const string whitelistPrefix = "Use Whitelist: ";
                const string enforceDifficultyPrefix = "Use Enforced Difficulty: ";
                const string wealthWarningPrefix = "Wealth Warning Threshold: ";
                const string wealthBanPrefix = "Wealth Ban Threshold: ";
                const string useWealthPrefix = "Use Wealth System: ";
                const string useIdleSysPrefix = "Use Idle System: ";
                const string idleTresholdPrefix = "Idle Threshold (days): ";
                const string useRoadSysPrefix = "Use Road System: ";
                const string aggressiveRoadModePrefix = "Aggressive Road Mode (WIP): ";
                const string modlistMatchPrefix = "Use Modlist Match: ";
                const string modlistConfigMatchPrefix = "Use Modlist Config Match (WIP): ";
                const string modVerifyPrefix = "Force Mod Verification: ";
                const string useChatPrefix = "Use Chat: ";
                const string profanityFilterPrefix = "Use Profanity filter: ";

                try
                {
                    foreach (string setting in settings)
                    {
                        if (setting.StartsWith(namePrefix))
                        {
                            serverConfig.ServerName = setting.Replace(namePrefix, string.Empty);
                        }
                        else if (setting.StartsWith(descriptionPrefix))
                        {
                            serverConfig.Description = setting.Replace(descriptionPrefix, string.Empty);
                        }
                        else if (setting.StartsWith(localIpPrefix))
                        {
                            serverConfig.HostIP = setting.Replace(localIpPrefix, string.Empty);
                        }
                        else if (setting.StartsWith(portPrefix))
                        {
                            serverConfig.Port = int.Parse(setting.Replace(portPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(maxPlayerPrefix))
                        {
                            serverConfig.MaxPlayers = ushort.Parse(setting.Replace(maxPlayerPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(devModePrefix))
                        {
                            serverConfig.AllowDevMode = bool.Parse(setting.Replace(devModePrefix, string.Empty));
                        }
                        else if (setting.StartsWith(enforceDifficultyPrefix))
                        {
                            serverConfig.ForceDifficulty = bool.Parse(setting.Replace(enforceDifficultyPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(whitelistPrefix))
                        {
                            serverConfig.WhitelistMode = bool.Parse(setting.Replace(whitelistPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(wealthWarningPrefix))
                        {
                            serverConfig.AntiCheat.WealthCheckSystem.WarningThreshold = int.Parse(setting.Replace(wealthWarningPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(wealthBanPrefix))
                        {
                            serverConfig.AntiCheat.WealthCheckSystem.BanThreshold = int.Parse(setting.Replace(wealthBanPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(useWealthPrefix))
                        {
                            serverConfig.AntiCheat.WealthCheckSystem.IsActive = bool.Parse(setting.Replace(useWealthPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(useIdleSysPrefix))
                        {
                            serverConfig.IdleSystem.IsActive = bool.Parse(setting.Replace(useIdleSysPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(idleTresholdPrefix))
                        {
                            serverConfig.IdleSystem.IdleThresholdInDays = uint.Parse(setting.Replace(idleTresholdPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(useRoadSysPrefix))
                        {
                            serverConfig.RoadSystem.IsActive = bool.Parse(setting.Replace(useRoadSysPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(aggressiveRoadModePrefix))
                        {
                            serverConfig.RoadSystem.AggressiveRoadMode = bool.Parse(setting.Replace(aggressiveRoadModePrefix, string.Empty));
                        }
                        else if (setting.StartsWith(modlistMatchPrefix))
                        {
                            serverConfig.ModsSystem.MatchModlist = bool.Parse(setting.Replace(modlistMatchPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(modlistConfigMatchPrefix))
                        {
                            serverConfig.ModsSystem.ModlistConfigMatch = bool.Parse(setting.Replace(modlistConfigMatchPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(modVerifyPrefix))
                        {
                            serverConfig.ModsSystem.ForceModVerification = bool.Parse(setting.Replace(modVerifyPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(useChatPrefix))
                        {
                            serverConfig.ChatSystem.IsActive = bool.Parse(setting.Replace(useChatPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(profanityFilterPrefix))
                        {
                            serverConfig.ChatSystem.UseProfanityFilter = bool.Parse(setting.Replace(profanityFilterPrefix, string.Empty));
                        }
                    }

                    File.Move(oldPath, Path.Combine(this.fileBackupPath, Path.GetFileName(oldPath)), true);
                    this.LogMigratedData(migrating, true);
                }
                catch (Exception ex)
                {
                    this.LogMigratedData(migrating, false, ex.Message);
                }
            }
        }

        public void MigrateWorldSettings(ServerConfig serverConfig)
        {
            var oldPath = Path.Combine(PathProvider.MainFolderPath, "World Settings.txt");
            if (File.Exists(oldPath))
            {
                const string migrating = "World Settings";
                this.EnsureBackupDir();
                this.LogMigratingData(migrating);

                var settings = File.ReadAllLines(oldPath);

                const string globalCoveragePrefix = "Globe Coverage (0.3, 0.5, 1.0): ";
                const string seedPrefix = "Seed: ";
                const string rainfallPrefix = "Overall Rainfall (0-6): ";
                const string tempPrefix = "Overall Temperature (0-6): ";
                const string populationPrefix = "Overall Population (0-6): ";

                try
                {
                    foreach (string setting in settings)
                    {
                        if (setting.StartsWith(globalCoveragePrefix))
                        {
                            serverConfig.Planet.GlobeCoverage = double.Parse(setting.Replace(globalCoveragePrefix, string.Empty), System.Globalization.NumberStyles.Number);
                        }
                        else if (setting.StartsWith(seedPrefix))
                        {
                            serverConfig.Planet.Seed = setting.Replace(seedPrefix, string.Empty);
                        }
                        else if (setting.StartsWith(rainfallPrefix))
                        {
                            serverConfig.Planet.OverallRainfall = byte.Parse(setting.Replace(rainfallPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(tempPrefix))
                        {
                            serverConfig.Planet.OverallTemperature = byte.Parse(setting.Replace(tempPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(populationPrefix))
                        {
                            serverConfig.Planet.OverallPopulation = byte.Parse(setting.Replace(populationPrefix, string.Empty));
                        }
                    }

                    File.Move(oldPath, Path.Combine(this.fileBackupPath, Path.GetFileName(oldPath)), true);
                    this.LogMigratedData(migrating, true);
                }
                catch (Exception ex)
                {
                    this.LogMigratedData(migrating, false, ex.Message);
                }
            }
        }

        private void MigrateWhitelist()
        {
            var oldPath = Path.Combine(PathProvider.MainFolderPath, "Whitelisted Players.txt");
            if (File.Exists(oldPath))
            {
                const string migrating = "Whitelisted Players";
                this.EnsureBackupDir();
                this.LogMigratingData(migrating);

                var players = File.ReadAllLines(oldPath);
                var playersList = players.ToList();

                JsonDataHelper.Save(playersList, PathProvider.PlayerWhitelistFile);
                File.Move(oldPath, Path.Combine(this.fileBackupPath, Path.GetFileName(oldPath)), true);
                this.LogMigratedData(migrating, true);
            }
        }

        private void MigrateBanlist()
        {
            var oldPath = Path.Combine(PathProvider.MainFolderPath, "bans_ip.dat");
            if (File.Exists(oldPath))
            {
                const string migrating = "Banned Players";
                this.EnsureBackupDir();
                this.LogMigratingData(migrating);

                var newBannedList = new List<BanInfo>();
                var formatter = new BinaryFormatter();
                using (var f = File.OpenRead(oldPath))
                {
                    var data = formatter.Deserialize(f) as BanDataHolder;
                    newBannedList = data.BannedIPs.Select(kp => new BanInfo()
                    {
                        Username = kp.Value,
                        IPAddress = kp.Key,
                    }).ToList();
                }

                JsonDataHelper.Save(newBannedList, PathProvider.BannedPlayersFile);
                File.Move(oldPath, Path.Combine(this.fileBackupPath, Path.GetFileName(oldPath)), true);
                this.LogMigratedData(migrating, true);
            }
        }

        private void MigrateModsFolder()
        {
            this.MigrateModFolder(Path.Combine(PathProvider.MainFolderPath, "Enforced Mods"), PathProvider.RequiredModsFolderPath, "Enforced Mods");
            this.MigrateModFolder(Path.Combine(PathProvider.MainFolderPath, "Whitelisted Mods"), PathProvider.WhitelistedModsFolderPath, "Whitelisted Mods");
            this.MigrateModFolder(Path.Combine(PathProvider.MainFolderPath, "Blacklisted Mods"), PathProvider.BlacklistedModsFolderPath, "Blacklisted Mods");
        }

        private void MigrateModFolder(string source, string target, string title)
        {
            if (Directory.Exists(source))
            {
                this.LogMigratingData(title);

                try
                {
                    if (Directory.Exists(target))
                    {
                        Directory.Delete(target, true);
                    }

                    Directory.Move(source, target);
                    this.LogMigratedData(title, true);
                }
                catch (Exception ex)
                {
                    this.LogMigratedData(title, false, ex.Message);
                }
            }
        }
    }
}
