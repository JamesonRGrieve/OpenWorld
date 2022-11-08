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
        private readonly WorldMapHandler worldMapHandler;

        public bool IsRunning { get; set; } = false;

        private TcpListener listener;

        //Meta
        public static bool exit = false;

        public static List<PlayerClient> savedClients = new List<PlayerClient>();

        //Server Details
        public Server(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;

            // Setting up Server
            this.modHandler = new ModHandler(serverConfig);
            this.playerHandler = new PlayerHandler(serverConfig);
            this.worldMapHandler = new WorldMapHandler(this.playerHandler);

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
            PlayerUtils.CheckAllAvailablePlayers();

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
                var needUpdate = false;
                while (this.IsRunning)
                {
                    foreach (var client in this.playerHandler.ConnectedClients.ToArray())
                    {
                        if (!client.IsConnected || client.IsDisconnecting)
                        {
                            this.playerHandler.RemovePlayer(client);
                            needUpdate = true;
                            continue;
                        }

                        if (client.DataAvailable)
                        {
                            Task.Run(() => this.ReadDataFromClient(client));
                        }
                    }

                    if (needUpdate)
                    {
                        ConsoleUtils.UpdateTitle();
                        ServerUtils.SendPlayerListToAll(null);
                        needUpdate = false;
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
                JoiningsUtils.LoginProcedures(client, PacketHandler.GetPacket<ConnectPacket>(data));
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
            StaticProxy.worldMapHandler = this.worldMapHandler;
        }

        public static void ListenForCommands()
        {
            string[] commandParts = Console.ReadLine().Trim().Split(" "), commandArgs = commandParts.TakeLast(commandParts.Length - 1).ToArray();
            string commandWord = commandParts[0].ToLower();
            if (string.Join(' ', commandArgs).Count(x => x == '"')%2 != 0) ConsoleUtils.LogToConsole("Uneven amount of quotation marks detected, aborting.", ConsoleUtils.ConsoleLogMode.Error);
            else
            {
                // TODO: The last IsNullOrWhiteSpace() is to deal with quotes at the end of command strings. Clean that up so it's not necessary.
                /*
                1. Recombine the commandArgs (split by spaces) to form the full entry string sans command word.
                2. Split it by quotation marks. This will make a list, alternating non-quoted and quoted portions of the command.
                3. Trim out the spaces left by splitting on quotation marks.
                4. Select either:
                    4a. If the item is even (non-quoted), split it by spaces into a new sub-array.
                    4b. If the item is odd (quoted), form a new sub-array that consists of the string as-is.
                5. Recombine the sub-lists to form a single list of split and quoted arguments.
                6. Remove the blank item that's created if the arg list ends with a quoted arg (see TODO). 
                7. Convert it to an array.
                */
                string[] processedCommandArgs = string.Join(' ', commandArgs).Split('"').Select(x => x.Trim()).Select((c, i) => i % 2 == 0 ? c.Split(' ') : new string[1] { c }).SelectMany(x => x).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                try
                {
                    Command invoked = ServerCommands.Where(x => x.Word == commandWord).SingleOrDefault();
                    if (invoked != null) invoked.Execute(processedCommandArgs);
                    else SimpleCommands.UnknownCommand(commandWord);
                }
                catch (Exception ex)
                {
                    ConsoleUtils.LogToConsole($"ERROR: {ex.Message}", ConsoleUtils.ConsoleLogMode.Error);
                }
            }
        }
    }
}