using System;
using System.Collections.Generic;
using System.Linq;
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
        public static string serverVersion = "v1.4.1 Unstable";


        //Server Mods
        public static List<string> enforcedMods = new List<string>();
        public static List<string> whitelistedMods = new List<string>();
        public static List<string> blacklistedMods = new List<string>();

        //Server Lists
        public static List<string> adminList = new List<string>();
        public static List<string> chatCache = new List<string>();
        public static List<Faction> savedFactions = new List<Faction>();

        public void Run()
        {
            AdoptConfigToStaticVars(this.serverConfig);
            this.SetupHandlerProxy();

            FactionHandler.CheckFactions(true);
            PlayerUtils.CheckAllAvailablePlayers(false);

            Threading.GenerateThreads(0);

            while (!exit) ListenForCommands();
        }

        private static void AdoptConfigToStaticVars(ServerConfig serverConfig)
        {
            // This needs to be replaced with proper use of the server config in the classes

            // Server Settings.txt
            Networking.localAddress = IPAddress.Parse(serverConfig.HostIP);
            Networking.serverPort = serverConfig.Port;

            // Mods
            Server.enforcedMods = StaticProxy.modHandler.RequiredMods.ToArray().Select(m => m.Name).ToList();
            Server.whitelistedMods = StaticProxy.modHandler.WhitelisteddMods.ToArray().Select(m => m.Name).ToList();
            Server.blacklistedMods = StaticProxy.modHandler.BlacklistedMods.ToArray().Select(m => m.Name).ToList();
        }

        public void SetupHandlerProxy()
        {
            StaticProxy.serverConfig = this.serverConfig;
            StaticProxy.modHandler = this.modHandler;
            StaticProxy.playerHandler = this.playerHandler;
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