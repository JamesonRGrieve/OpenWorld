using System;
using System.Collections.Generic;
using System.Net;
using OpenWorldServer.Data;
using OpenWorldServer.Handlers;

namespace OpenWorldServer
{
    [System.Serializable]
    public class Server
    {
        private readonly ServerConfig serverConfig;
        private readonly PlayerHandler playerHandler;
        private readonly ModHandler modHandler;

        public Server(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;

            // Setting up Server
            this.modHandler = new ModHandler(serverConfig);
            this.playerHandler = new PlayerHandler(serverConfig);

            this.Run();
        }

        //Meta
        public static bool exit = false;

        //Player Parameters
        public static List<ServerClient> savedClients = new List<ServerClient>();
        public static Dictionary<string, List<string>> savedSettlements = new Dictionary<string, List<string>>();

        //Server Details
        public static string serverName = "";
        public static string serverDescription = "";
        public static string serverVersion = "v1.4.1 Unstable";

        //Server Variables
        public static int maxPlayers = 300;
        public static int warningWealthThreshold = 10000;
        public static int banWealthThreshold = 100000;
        public static int idleTimer = 7;

        //Server Booleans
        public static bool usingIdleTimer = false;
        public static bool allowDevMode = false;
        public static bool usingWhitelist = false;
        public static bool usingWealthSystem = false;
        public static bool usingRoadSystem = false;
        public static bool aggressiveRoadMode = false;
        public static bool forceModlist = false;
        public static bool forceModlistConfigs = false;
        public static bool usingModVerification = false;
        public static bool usingChat = false;
        public static bool usingProfanityFilter = false;

        //Server Mods
        public static List<string> enforcedMods = new List<string>();
        public static List<string> whitelistedMods = new List<string>();
        public static List<string> blacklistedMods = new List<string>();

        //Server Lists
        public static List<string> whitelistedUsernames = new List<string>();
        public static List<string> adminList = new List<string>();
        public static List<string> chatCache = new List<string>();
        public static Dictionary<string, string> bannedIPs = new Dictionary<string, string>();
        public static List<Faction> savedFactions = new List<Faction>();

        //World Parameters
        public static double globeCoverage;
        public static string seed;
        public static int overallRainfall;
        public static int overallTemperature;
        public static int overallPopulation;

        public void Run()
        {
            AdoptConfigToStaticVars(this.serverConfig);
            Server.whitelistedUsernames = this.playerHandler.PlayerWhitelist.Usernames;

            FactionHandler.CheckFactions(true);
            PlayerUtils.CheckAllAvailablePlayers(false);

            Threading.GenerateThreads(0);

            while (!exit) ListenForCommands();
        }

        private static void AdoptConfigToStaticVars(ServerConfig serverConfig)
        {
            // This needs to be replaced with proper use of the server config in the classes

            // Server Settings.txt
            Server.serverName = serverConfig.ServerName;
            Server.serverDescription = serverConfig.Description;
            Networking.localAddress = IPAddress.Parse(serverConfig.HostIP);
            Networking.serverPort = serverConfig.Port;
            Server.maxPlayers = serverConfig.MaxPlayers;
            Server.allowDevMode = serverConfig.AllowDevMode;
            Server.usingWhitelist = serverConfig.WhitelistMode;
            Server.warningWealthThreshold = serverConfig.AntiCheat.WealthCheckSystem.WarningThreshold;
            Server.banWealthThreshold = serverConfig.AntiCheat.WealthCheckSystem.BanThreshold;
            Server.usingWealthSystem = serverConfig.AntiCheat.WealthCheckSystem.IsActive;
            Server.usingIdleTimer = serverConfig.IdleSystem.IsActive;
            Server.idleTimer = (int)serverConfig.IdleSystem.IdleThresholdInDays;
            Server.usingRoadSystem = serverConfig.RoadSystem.IsActive;
            Server.aggressiveRoadMode = serverConfig.RoadSystem.AggressiveRoadMode;
            Server.forceModlist = serverConfig.ModsSystem.MatchModlist;
            Server.forceModlistConfigs = serverConfig.ModsSystem.ModlistConfigMatch;
            Server.usingModVerification = serverConfig.ModsSystem.ForceModVerification;
            Server.usingChat = serverConfig.ChatSystem.IsActive;
            Server.usingProfanityFilter = serverConfig.ChatSystem.UseProfanityFilter;

            // World Settings.txt
            Server.globeCoverage = serverConfig.World.GlobeCoverage;
            Server.seed = serverConfig.World.Seed;
            Server.overallRainfall = serverConfig.World.OverallRainfall;
            Server.overallTemperature = serverConfig.World.OverallTemperature;
            Server.overallPopulation = serverConfig.World.OverallPopulation;
        }

        public static void ListenForCommands()
        {
            Console.ForegroundColor = ConsoleColor.White;

            string fullCommand = Console.ReadLine();
            string commandBase = fullCommand.Split(' ')[0].ToLower();

            string commandArguments = "";
            if (fullCommand.Contains(' ')) commandArguments = fullCommand.Replace(fullCommand.Split(' ')[0], "").Remove(0, 1);

            Dictionary<string, Action> simpleCommands = new Dictionary<string, Action>()
            {
                {"help", SimpleCommands.HelpCommand},
                {"settings", SimpleCommands.SettingsCommand},
                {"modlist", SimpleCommands.ModListCommand},
                {"reload", SimpleCommands.ReloadCommand},
                {"status", SimpleCommands.StatusCommand},
                {"eventlist", SimpleCommands.EventListCommand},
                {"chat", SimpleCommands.ChatCommand},
                {"list", SimpleCommands.ListCommand},
                {"settlements", SimpleCommands.SettlementsCommand},
                {"banlist", SimpleCommands.BanListCommand},
                {"adminlist", SimpleCommands.AdminListCommand},
                {"whitelist", SimpleCommands.WhiteListCommand},
                {"wipe", SimpleCommands.WipeCommand},
                {"clear", SimpleCommands.ClearCommand},
                {"exit", SimpleCommands.ExitCommand}
            };

            Dictionary<string, Action> advancedCommands = new Dictionary<string, Action>()
            {
                {"say", AdvancedCommands.SayCommand},
                {"broadcast", AdvancedCommands.BroadcastCommand},
                {"notify", AdvancedCommands.NotifyCommand},
                {"invoke", AdvancedCommands.InvokeCommand},
                {"plague", AdvancedCommands.PlagueCommand},
                {"player", AdvancedCommands.PlayerDetailsCommand},
                {"faction", AdvancedCommands.FactionDetailsCommand},
                {"kick", AdvancedCommands.KickCommand},
                {"ban", AdvancedCommands.BanCommand},
                {"pardon", AdvancedCommands.PardonCommand},
                {"promote", AdvancedCommands.PromoteCommand},
                {"demote", AdvancedCommands.DemoteCommand},
                {"giveitem", AdvancedCommands.GiveItemCommand},
                {"giveitemall", AdvancedCommands.GiveItemAllCommand},
                {"protect", AdvancedCommands.ProtectCommand},
                {"deprotect", AdvancedCommands.DeprotectCommand},
                {"immunize", AdvancedCommands.ImmunizeCommand},
                {"deimmunize", AdvancedCommands.DeimmunizeCommand}
            };

            if (simpleCommands.ContainsKey(commandBase))
            {
                simpleCommands[commandBase]();
            }

            else if (advancedCommands.ContainsKey(commandBase))
            {
                AdvancedCommands.commandData = commandArguments;
                advancedCommands[commandBase]();
            }

            else SimpleCommands.UnknownCommand(commandBase);
        }
    }
}