using System;
using System.IO;
using System.Text.Json;
using OpenWorldServer.Data;
using OpenWorldServer.Services;

namespace OpenWorldServer.Migrations
{
    internal class MigrationService
    {
        private string fileBackupPath = Path.Combine(PathProvider.MainFolderPath, "MigrationBackups");

        public void MigrateAll()
        {
            this.MigrateConfig();
        }

        public static MigrationService CreateAndMigrateAll()
        {
            var service = new MigrationService();
            service.MigrateAll();
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

        public void MigrateConfig()
        {
            var oldPath = Path.Combine(PathProvider.MainFolderPath, "Server Settings.txt");
            if (File.Exists(oldPath))
            {
                const string migrating = "Server Settings";
                this.EnsureBackupDir();
                this.LogMigratingData(migrating);

                var newConfig = new ServerConfig();
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

                foreach (string setting in settings)
                {
                    if (setting.StartsWith(namePrefix))
                    {
                        newConfig.ServerName = setting.Replace(namePrefix, string.Empty);
                    }
                    else if (setting.StartsWith(descriptionPrefix))
                    {
                        newConfig.Description = setting.Replace(descriptionPrefix, string.Empty);
                    }
                    else if (setting.StartsWith(localIpPrefix))
                    {
                        newConfig.HostIP = setting.Replace(localIpPrefix, string.Empty);
                    }
                    else if (setting.StartsWith(portPrefix))
                    {
                        newConfig.Port = int.Parse(setting.Replace(portPrefix, string.Empty));
                    }
                    else if (setting.StartsWith(maxPlayerPrefix))
                    {
                        newConfig.MaxPlayers = ushort.Parse(setting.Replace(maxPlayerPrefix, string.Empty));
                    }
                    else if (setting.StartsWith(devModePrefix))
                    {
                        newConfig.AllowDevMode = bool.Parse(setting.Replace(devModePrefix, string.Empty));
                    }
                    else if (setting.StartsWith(whitelistPrefix))
                    {
                        newConfig.WhitelistMode = bool.Parse(setting.Replace(whitelistPrefix, string.Empty));
                    }
                    else if (setting.StartsWith(wealthWarningPrefix))
                    {
                        newConfig.AntiCheat.WealthCheckSystem.WarningThreshold = int.Parse(setting.Replace(wealthWarningPrefix, string.Empty));
                    }
                    else if (setting.StartsWith(wealthBanPrefix))
                    {
                        newConfig.AntiCheat.WealthCheckSystem.BanThreshold = int.Parse(setting.Replace(wealthBanPrefix, string.Empty));
                    }
                    else if (setting.StartsWith(useWealthPrefix))
                    {
                        newConfig.AntiCheat.WealthCheckSystem.IsActive = bool.Parse(setting.Replace(useWealthPrefix, string.Empty));
                    }
                    else if (setting.StartsWith(useIdleSysPrefix))
                    {
                        newConfig.IdleSystem.IsActive = bool.Parse(setting.Replace(useIdleSysPrefix, string.Empty));
                    }
                    else if (setting.StartsWith(idleTresholdPrefix))
                    {
                        newConfig.IdleSystem.IdleThresholdInDays = uint.Parse(setting.Replace(idleTresholdPrefix, string.Empty));
                    }
                    else if (setting.StartsWith(useRoadSysPrefix))
                    {
                        newConfig.RoadSystem.IsActive = bool.Parse(setting.Replace(useRoadSysPrefix, string.Empty));
                    }
                    else if (setting.StartsWith(aggressiveRoadModePrefix))
                    {
                        newConfig.RoadSystem.AggressiveRoadMode = bool.Parse(setting.Replace(aggressiveRoadModePrefix, string.Empty));
                    }
                    else if (setting.StartsWith(modlistMatchPrefix))
                    {
                        newConfig.ModsSystem.MatchModlist = bool.Parse(setting.Replace(modlistMatchPrefix, string.Empty));
                    }
                    else if (setting.StartsWith(modlistConfigMatchPrefix))
                    {
                        newConfig.ModsSystem.ModlistConfigMatch = bool.Parse(setting.Replace(modlistConfigMatchPrefix, string.Empty));
                    }
                    else if (setting.StartsWith(modVerifyPrefix))
                    {
                        newConfig.ModsSystem.ForceModVerification = bool.Parse(setting.Replace(modVerifyPrefix, string.Empty));
                    }
                    else if (setting.StartsWith(useChatPrefix))
                    {
                        newConfig.ChatSystem.IsActive = bool.Parse(setting.Replace(useChatPrefix, string.Empty));
                    }
                    else if (setting.StartsWith(profanityFilterPrefix))
                    {
                        newConfig.ChatSystem.UseProfanityFilter = bool.Parse(setting.Replace(profanityFilterPrefix, string.Empty));
                    }
                }

                if (File.Exists(PathProvider.ConfigFile))
                {
                    Console.WriteLine($"{Path.GetFileName(PathProvider.ConfigFile)} already exists. Overwrite? Y/n");
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.N)
                    {
                        this.LogMigratedData(migrating, false, "Aborted");
                        return;
                    }
                    Console.WriteLine();
                }

                File.WriteAllText(PathProvider.ConfigFile, JsonSerializer.Serialize(newConfig, new JsonSerializerOptions() { WriteIndented = true }));
                File.Move(oldPath, Path.Combine(this.fileBackupPath, Path.GetFileName(oldPath)), true);
                this.LogMigratedData(migrating, true);
            }
        }
    }
}
