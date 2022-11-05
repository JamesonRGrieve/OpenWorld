using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using OpenWorld.Shared.Networking;
using OpenWorld.Shared.Networking.Packets;
using OpenWorldServer.Data;
using OpenWorldServer.Handlers;
using OpenWorldServer.Handlers.Old;

namespace OpenWorldServer
{
    [System.Serializable]
    public class Server
    {
        private readonly ServerConfig serverConfig;
        private readonly PlayerHandler playerHandler;
        private readonly ModHandler modHandler;

        public bool IsRunning { get; set; } = false;

        private TcpListener listener;

        //Meta
        public static bool exit = false;

        //Player Parameters
        public static List<PlayerClient> savedClients = new List<PlayerClient>();

        //Server Details
        public static string serverVersion = "v1.4.1";

        //Server Lists
        public static List<string> adminList = new List<string>();
        public static List<string> chatCache = new List<string>();
        public static List<Faction> savedFactions = new List<Faction>();

        public static string latestClientVersion;

        public Server(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;

            // Setting up Server
            this.modHandler = new ModHandler(serverConfig);
            this.playerHandler = new PlayerHandler(serverConfig);

            this.SetupStaticProxy();

            this.SetupListener();

            this.Run();
        }

        private void SetupListener()
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(this.serverConfig.HostIP, out ipAddress))
            {
                ConsoleUtils.LogToConsole($"The IP [{this.serverConfig.HostIP}] is not a valid IP", ConsoleColor.Red);
                return;
            }

            if (this.serverConfig.Port >= 65535 || this.serverConfig.Port <= 0)
            {
                ConsoleUtils.LogToConsole($"The Port [{this.serverConfig.Port}] needs to be between 0 and 65535", ConsoleColor.Red);
                return;
            }

            this.listener = new TcpListener(ipAddress, this.serverConfig.Port);
            this.listener.Start();
        }

        public void Run()
        {
            this.IsRunning = true;

            this.StartReadingDataFromClients();
            this.StartKeepAliveChecks();
            this.StartAcceptingConnections();

            FactionHandler.CheckFactions(true);
            PlayerUtils.CheckAllAvailablePlayers(false);

            Threading.GenerateThreads(0);

            while (!exit) ListenForCommands();
        }

        private void StartAcceptingConnections()
        {
            Task.Run(() =>
            {
                ConsoleUtils.LogToConsole($"Server ready to accept connections");
                while (this.IsRunning)
                {
                    try
                    {
                        var newClient = this.listener.AcceptTcpClient();
                        this.playerHandler.AddPlayer(newClient);

                        // We actually dont need to delay the Thread here, since AcceptTcpClient is blocking
                        // But we dont want to rush through many clients on a mass reconnect.
                        // Modern CPUs shouldn't have problems, but a pi~
                        Thread.Sleep(120);
                    }
                    catch (Exception ex)
                    {
                        if (this.IsRunning)
                        {
                            ConsoleUtils.LogToConsole($"Exception while trying to accept new connection:", ConsoleColor.Red);
                            ConsoleUtils.LogToConsole(ex.Message, ConsoleColor.Red);
                        }
                    }
                }
            });
        }

        private void StartReadingDataFromClients()
        {
            Task.Run(() =>
            {
                while (this.IsRunning)
                {
                    foreach (var client in this.playerHandler.ConnectedClients.ToArray())
                    {
                        if (!client.IsConnected || client.IsDisconnecting)
                        {
                            this.playerHandler.RemovePlayer(client);
                        }

                        if (client.DataAvailable)
                        {
                            Task.Run(() => this.ReadDataFromClient(client));
                        }
                    }

                    Thread.Sleep(50); // Let the CPU do some other things
                }
            });
        }

        private void StartKeepAliveChecks()
        {
            Task.Run(() =>
            {
                var pingPacketData = new PingPacket().GetData();
                while (this.IsRunning)
                {
                    var clients = this.playerHandler.ConnectedClients.ToArray();

                    if (clients.Length == 0)
                    {
                        Thread.Sleep(500);
                        continue;
                    }

                    foreach (var client in clients)
                    {
                        if (!client.IsConnected || client.IsDisconnecting)
                        {
                            this.playerHandler.RemovePlayer(client);
                        }

                        Task.Run(() => client.SendData(pingPacketData));
                    }

                    Thread.Sleep(1000); // Let the CPU do some other things
                }
            });
        }

        private void ReadDataFromClient(PlayerClient client)
        {
            string data = null;
            try
            {
                data = client.ReceiveData();
            }
            catch { }

            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            if (data.StartsWith("Connect│"))
            {
                NetworkingHandler.ConnectHandle(client, PacketHandler.GetPacket<ConnectPacket>(data));
            }
            else if (data.StartsWith("ChatMessage│"))
            {
                NetworkingHandler.ChatMessageHandle(client, data);
            }
            else if (data.StartsWith("UserSettlement│"))
            {
                NetworkingHandler.UserSettlementHandle(client, data);
            }
            else if (data.StartsWith("ForceEvent│"))
            {
                NetworkingHandler.ForceEventHandle(client, data);
            }
            else if (data.StartsWith("SendGiftTo│"))
            {
                NetworkingHandler.SendGiftHandle(client, data);
            }
            else if (data.StartsWith("SendTradeTo│"))
            {
                NetworkingHandler.SendTradeHandle(client, data);
            }
            else if (data.StartsWith("SendBarterTo│"))
            {
                NetworkingHandler.SendBarterHandle(client, data);
            }
            else if (data.StartsWith("TradeStatus│"))
            {
                NetworkingHandler.TradeStatusHandle(client, data);
            }
            else if (data.StartsWith("BarterStatus│"))
            {
                NetworkingHandler.BarterStatusHandle(client, data);
            }
            else if (data.StartsWith("GetSpyInfo│"))
            {
                NetworkingHandler.SpyInfoHandle(client, data);
            }
            else if (data.StartsWith("FactionManagement│"))
            {
                NetworkingHandler.FactionManagementHandle(client, data);
            }
        }

        public void SetupStaticProxy()
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

            try
            {
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

            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleUtils.WriteWithTime("Command Caught Exception");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}