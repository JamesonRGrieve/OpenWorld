using System;
using System.IO;
using System.Linq;
using OpenWorldServer.Data;
using OpenWorldServer.Utils;

namespace OpenWorldServer.Services
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
            this.MigrateModsFolder();
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
            => ConsoleUtils.LogToConsole($"Migrating {data}", ConsoleColor.Yellow);

        private void LogMigratedData(string data, bool successfully, string reason = "")
        {
            var state = successfully ? "successfully" : "failed";
            var color = successfully ? ConsoleColor.Green : ConsoleColor.Red;
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
                            serverConfig.World.GlobeCoverage = double.Parse(setting.Replace(globalCoveragePrefix, string.Empty), System.Globalization.NumberStyles.Number);
                        }
                        else if (setting.StartsWith(seedPrefix))
                        {
                            serverConfig.World.Seed = setting.Replace(seedPrefix, string.Empty);
                        }
                        else if (setting.StartsWith(rainfallPrefix))
                        {
                            serverConfig.World.OverallRainfall = byte.Parse(setting.Replace(rainfallPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(tempPrefix))
                        {
                            serverConfig.World.OverallTemperature = byte.Parse(setting.Replace(tempPrefix, string.Empty));
                        }
                        else if (setting.StartsWith(populationPrefix))
                        {
                            serverConfig.World.OverallPopulation = byte.Parse(setting.Replace(populationPrefix, string.Empty));
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
                var playerWhitelist = new PlayerWhitelist();
                playerWhitelist.Usernames = players.ToList();

                JsonDataHelper.Save(playerWhitelist, PathProvider.PlayerWhitelistFile);
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
