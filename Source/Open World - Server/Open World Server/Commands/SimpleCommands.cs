using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenWorldServer.Handlers.Old;

namespace OpenWorldServer
{
    public static class SimpleCommands
    {
        public static void HelpCommand()
        {
            Console.Clear();
            ConsoleUtils.LogToConsole("List of Available Commands", ConsoleUtils.ConsoleLogMode.Heading);
            foreach (string category in Enum.GetNames(typeof(Command.CommandCategory)))
            {
                ConsoleUtils.LogToConsole(category.Replace('_', ' '), ConsoleUtils.ConsoleLogMode.Heading);
                ConsoleUtils.LogToConsole(
                string.Join('\n', Server.ServerCommands.Where(x => x.Category.ToString() == category).Select(x => $"{x.Word}: {x.Description}" +
                    (x.AdvancedCommand != null
                        ? $"\n\tUsage: {x.Word} {string.Join(' ', x.Parameters.Select(y => $"[{y.Name.ToLower()}]"))}\n\tParameters:\n{string.Join('\n', x.Parameters.Select(y => $"\t\t-{y.Name}: {y.Description}"))}"
                        : ""
                    )
                )));
            }

        }
        private static readonly Dictionary<string, Dictionary<string, string>> SETTINGS = new Dictionary<string, Dictionary<string, string>>()
        {
            { "Server Settings", new Dictionary<string, string>()
                {
                    {"Server Name", StaticProxy.serverConfig.ServerName },
                    {"Server Description", StaticProxy.serverConfig.Description },
                    {"Server Local IP", StaticProxy.serverConfig.HostIP },
                    {"Server Port", StaticProxy.serverConfig.Port.ToString() },
                }
            },
            { "World Settings", new Dictionary<string, string>()
                {
                    {"Globe Coverage", StaticProxy.serverConfig.Planet.GlobeCoverage.ToString() },
                    {"Seed", StaticProxy.serverConfig.Planet.Seed },
                    {"Overall Rainfall", StaticProxy.serverConfig.Planet.OverallRainfall.ToString() },
                    {"Overall Temperature", StaticProxy.serverConfig.Planet.OverallTemperature.ToString() },
                    { "Overall Population", StaticProxy.serverConfig.Planet.OverallPopulation.ToString()}
                }
            }
        };
        public static void SettingsCommand()
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> setting in SETTINGS)
            {
                ConsoleUtils.LogToConsole(setting.Key, ConsoleUtils.ConsoleLogMode.Heading);
                ConsoleUtils.LogToConsole(string.Join('\n', setting.Value.Select(x => $"{x.Key}: {x.Value}")));
            }
        }
        private static readonly Dictionary<string, List<string>> MOD_LIST = new Dictionary<string, List<string>>()
        {
            { "Enforced Mods", StaticProxy.modHandler.RequiredMods.ToArray().Select(m => m.Name).ToList() },
            { "Whitelisted Mods", StaticProxy.modHandler.WhitelistedMods.ToArray().Select(m => m.Name).ToList() },
            { "Blacklisted Mods", StaticProxy.modHandler.BlacklistedMods.ToArray().Select(m => m.Name).ToList() }
        };
        public static void ModListCommand()
        {
            foreach (KeyValuePair<string, List<string>> subList in MOD_LIST)
            {
                ConsoleUtils.LogToConsole($"{subList.Key}: {subList.Value.Count}", ConsoleUtils.ConsoleLogMode.Heading);
                ConsoleUtils.LogToConsole(subList.Value.Count == 0 ? $"No {subList.Key}" : string.Join('\n', subList.Value));
            }
        }
        public static void ExitCommand()
        {
            foreach (PlayerClient sc in StaticProxy.playerHandler.ConnectedClients)
            {
                Networking.SendData(sc, "Disconnect│Closing");
                sc.IsDisconnecting = true;
            }
            Server.exit = true;
        }
        public static void ClearCommand()
        {
            Console.Clear();
        }

        public static void ReloadCommand()
        {
            StaticProxy.modHandler.ReloadModFolders();
            StaticProxy.playerHandler.AccountsHandler.ReloadAccounts();
            FactionHandler.CheckFactions();
            PlayerUtils.CheckAllAvailablePlayers();
        }

        private static readonly Dictionary<string, Dictionary<string, string>> STATUSES_1 = new Dictionary<string, Dictionary<string, string>>()
        {
            { "Server Status", new Dictionary<string, string>()
                {
                    {"Version", Server.serverVersion },
                    {"Live", "True" },
                    {"Uptime", (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).ToString() }
                }
            },
            {   "Mod List Status", new Dictionary<string, string>()
                {
                    {"Using Modlist Check", StaticProxy.serverConfig.ModsSystem.MatchModlist.ToString() },
                    {"Using Modlist Config Check", StaticProxy.serverConfig.ModsSystem.ModlistConfigMatch.ToString() },
                    {"Using Mod Verification", StaticProxy.serverConfig.ModsSystem.ToString() }
                }
            }
        };
        private static readonly Dictionary<string, Dictionary<string, string>> STATUSES_2 = new Dictionary<string, Dictionary<string, string>>()
        {
            { "Chat", new Dictionary<string, string>()
                {
                    {"Using Chat", StaticProxy.serverConfig.ChatSystem.IsActive.ToString() },
                    {"Using Profanity Filter", StaticProxy.serverConfig.ChatSystem.UseProfanityFilter.ToString() }
                }
            },
            {   "Wealth", new Dictionary<string, string>()
                {
                    {"Using Wealth System", StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.IsActive.ToString() },
                    {"Warning Threshold", StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.WarningThreshold.ToString() },
                    {"Ban Threshold", StaticProxy.serverConfig.AntiCheat.WealthCheckSystem.BanThreshold.ToString() }
                }
            },
            {   "Idle", new Dictionary<string, string>()
                {
                    {"Using Idle System", StaticProxy.serverConfig.IdleSystem.IsActive.ToString() },
                    {"Idle Threshold", StaticProxy.serverConfig.IdleSystem.IdleThresholdInDays.ToString() }
                }
            },
            {   "Road", new Dictionary<string, string>()
                {
                    {"Using Road System", StaticProxy.serverConfig.RoadSystem.IsActive.ToString() },
                    {"Aggressive Road Mode", StaticProxy.serverConfig.RoadSystem.AggressiveRoadMode.ToString() }
                }
            },
            {   "Miscellaneous", new Dictionary<string, string>()
                {
                    {"Using Enforced Difficulty", StaticProxy.serverConfig.ForceDifficulty.ToString() },
                    {"Allow Dev Mode", StaticProxy.serverConfig.AllowDevMode.ToString() }
                }
            }
        };
        public static void StatusCommand()
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> status in STATUSES_1)
            {
                ConsoleUtils.LogToConsole(status.Key, ConsoleUtils.ConsoleLogMode.Heading);
                ConsoleUtils.LogToConsole(string.Join('\n', status.Value.Select(x => $"{x.Key}: {x.Value}")));
            }
            ModListCommand();
            ConsoleUtils.LogToConsole("Players", ConsoleUtils.ConsoleLogMode.Heading);
            ListCommand();
            SettlementsCommand();
            ConsoleUtils.LogToConsole("Using Whitelist: " + StaticProxy.serverConfig.WhitelistMode);
            WhiteListCommand();
            BanListCommand();
            foreach (KeyValuePair<string, Dictionary<string, string>> status in STATUSES_2)
            {
                ConsoleUtils.LogToConsole(status.Key, ConsoleUtils.ConsoleLogMode.Heading);
                ConsoleUtils.LogToConsole(string.Join('\n', status.Value.Select(x => $"{x.Key}: {x.Value}")));
            }
        }
        public static void WhiteListCommand()
        {
            ConsoleUtils.LogToConsole("Whitelisted Players: " + StaticProxy.playerHandler.WhitelistHandler.Whitelist.Count, ConsoleUtils.ConsoleLogMode.Heading);
            ConsoleUtils.LogToConsole(StaticProxy.playerHandler.WhitelistHandler.Whitelist.Count == 0 ? "No Whitelisted Players Found" : string.Join('\n', StaticProxy.playerHandler.WhitelistHandler.Whitelist.ToArray()));
        }
        public static void AdminListCommand()
        {
            List<PlayerClient> admins = Server.savedClients.Where(x => x.Account.IsAdmin).ToList();
            ConsoleUtils.LogToConsole("Server Administrators: " + admins.Count, ConsoleUtils.ConsoleLogMode.Heading);
            ConsoleUtils.LogToConsole(admins.Count == 0 ? "No Admins Found" : string.Join('\n', admins.Select(x => x.Account.Username)));
        }
        public static void BanListCommand()
        {
            ConsoleUtils.LogToConsole($"Banned players: {StaticProxy.playerHandler.BanlistHandler.Banlist.Count}", ConsoleUtils.ConsoleLogMode.Heading);
            ConsoleUtils.LogToConsole(StaticProxy.playerHandler.BanlistHandler.Banlist.Count == 0 ? "No Banned Players" : string.Join('\n', StaticProxy.playerHandler.BanlistHandler.Banlist.Select(x => $"[{x.Username}] - [{x.IPAddress}]")));
        }
        public static void WipeCommand()
        {
            ConsoleUtils.LogToConsole("WARNING! THIS ACTION WILL DELETE ALL PLAYER DATA. DO YOU WANT TO PROCEED? (Y/N)", ConsoleUtils.ConsoleLogMode.Warning);
            if (string.Equals(Console.ReadLine().Trim(), "Y", StringComparison.OrdinalIgnoreCase))
            {
                foreach (PlayerClient client in StaticProxy.playerHandler.ConnectedClients) client.IsDisconnecting = true;
                foreach (PlayerClient client in Server.savedClients)
                {
                    client.Account.Wealth = 0;
                    client.Account.PawnCount = 0;
                    StaticProxy.playerHandler.AccountsHandler.SaveAccount(client);
                }
                ConsoleUtils.LogToConsole("All Player Files Have Been Set To Wipe", ConsoleUtils.ConsoleLogMode.Info);
            }
            else ConsoleUtils.LogToConsole("Aborted Wipe Attempt", ConsoleUtils.ConsoleLogMode.Info);
        }
        public static void ListCommand()
        {
            ConsoleUtils.LogToConsole($"Connected Players: {StaticProxy.playerHandler.ConnectedClients.Count}", ConsoleUtils.ConsoleLogMode.Heading);
            if (StaticProxy.playerHandler.ConnectedClients.Count == 0) ConsoleUtils.LogToConsole("No Players Connected");
            else
            {
                foreach (PlayerClient client in StaticProxy.playerHandler.ConnectedClients)
                {
                    try
                    {
                        ConsoleUtils.LogToConsole(client.Account.Username);
                    }
                    catch
                    {
                        ConsoleUtils.LogToConsole($"Error Processing Player With IP {client.IPAddress}", ConsoleUtils.ConsoleLogMode.Error);
                    }
                }
            }
            ConsoleUtils.LogToConsole($"Saved Players: {Server.savedClients.Count}", ConsoleUtils.ConsoleLogMode.Heading);
            if (Server.savedClients.Count == 0) ConsoleUtils.LogToConsole("No Players Saved");
            else
            {
                foreach (PlayerClient savedClient in Server.savedClients)
                {
                    try
                    {
                        ConsoleUtils.LogToConsole(savedClient.Account.Username);
                    }
                    catch
                    {
                        ConsoleUtils.LogToConsole($"Error Processing Player With IP {savedClient.IPAddress}", ConsoleUtils.ConsoleLogMode.Error);
                    }
                }
            }
            ConsoleUtils.LogToConsole("Saved Factions: " + Server.savedFactions.Count, ConsoleUtils.ConsoleLogMode.Heading);
            ConsoleUtils.LogToConsole(Server.savedFactions.Count == 0 ? "No Factions Saved" : string.Join('\n', Server.savedFactions.Select(x => x.name)));
        }
        public static void SettlementsCommand()
        {
            ConsoleUtils.LogToConsole("Server Settlements: " + StaticProxy.worldMapHandler.GetAccountsWithSettlements.Count, ConsoleUtils.ConsoleLogMode.Heading);
            ConsoleUtils.LogToConsole(StaticProxy.worldMapHandler.GetAccountsWithSettlements.Count == 0 ? "No Settlements Saved" : string.Join('\n', StaticProxy.worldMapHandler.GetAccountsWithSettlements.Select(x => $"[{x.Username}] @ [{x.HomeTileId}]")));
        }
        public static readonly string[] EventList = new string[] { "Raid", "Infestation", "MechCluster", "ToxicFallout", "Manhunter", "Wanderer", "FarmAnimals", "ShipChunk", "GiveQuest", "TraderCaravan" };
        public static void EventListCommand()
        {
            ConsoleUtils.LogToConsole("List Of Available Events", ConsoleUtils.ConsoleLogMode.Heading);
            ConsoleUtils.LogToConsole(string.Join('\n', EventList));
        }
        public static void UnknownCommand(string command) => ConsoleUtils.LogToConsole("Command [" + command + "] Not Found", ConsoleUtils.ConsoleLogMode.Warning);
    }
}