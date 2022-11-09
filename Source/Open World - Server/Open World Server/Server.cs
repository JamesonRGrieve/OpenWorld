using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using OpenWorld.Shared.Networking.Packets;
using OpenWorldServer.Data;
using OpenWorldServer.Handlers;
using OpenWorldServer.Handlers.Old;

namespace OpenWorldServer
{
    public class Server
    {
        private readonly ServerConfig serverConfig;
        private readonly PlayerHandler playerHandler;
        private readonly ModHandler modHandler;
        private readonly WorldMapHandler worldMapHandler;
        private readonly ConnectionHandler connectionHandler;

        public bool IsRunning { get; set; } = false;

        private TcpListener listener;

        //Meta

        public static bool exit = false;

        //Player Parameters
        public static List<PlayerClient> savedClients = new List<PlayerClient>();

        //Server Details
        public static string serverVersion = "v1.4.2";

        //Server Lists
        public static List<string> adminList = new List<string>();
        public static List<string> chatCache = new List<string>();
        public static List<Faction> savedFactions = new List<Faction>();

        public static string latestClientVersion;

        //Server Details
        public Server(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;

            // Setting up Server
            this.modHandler = new ModHandler(serverConfig);
            this.playerHandler = new PlayerHandler(serverConfig);
            this.worldMapHandler = new WorldMapHandler(this.playerHandler);
            this.connectionHandler = new ConnectionHandler(this.serverConfig, this.playerHandler, this.modHandler, this.worldMapHandler);

            this.SetupStaticProxy();

            this.SetupListener();

            this.Run();
        }

        private void SetupListener()
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(this.serverConfig.HostIP, out ipAddress))
            {
                ConsoleUtils.LogToConsole($"The IP [{this.serverConfig.HostIP}] is not a valid IP", ConsoleUtils.ConsoleLogMode.Error);
                return;
            }

            if (this.serverConfig.Port >= 65535 || this.serverConfig.Port <= 0)
            {
                ConsoleUtils.LogToConsole($"The Port [{this.serverConfig.Port}] needs to be between 0 and 65535", ConsoleUtils.ConsoleLogMode.Error);
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

            FactionHandler.CheckFactions();
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
                            ConsoleUtils.LogToConsole($"Exception while trying to accept new connection:", ConsoleUtils.ConsoleLogMode.Error);
                            ConsoleUtils.LogToConsole(ex.Message, ConsoleUtils.ConsoleLogMode.Error);
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
                            Task.Run(() => this.connectionHandler.ReadDataFromClient(client));
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
            if (string.Join(' ', commandArgs).Count(x => x == '"') % 2 != 0) ConsoleUtils.LogToConsole("Uneven amount of quotation marks detected, aborting.", ConsoleUtils.ConsoleLogMode.Error);
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

        public static List<Command> ServerCommands = new List<Command>()
        {
            new Command()
            {
                Word = "help",
                Description = "Display all available commands.",
                Category = Command.CommandCategory.Information,
                SimpleCommand = SimpleCommands.HelpCommand
            },
            new Command()
            {
                Word = "settings",
                Description = "Displays the current server settings.",
                Category = Command.CommandCategory.Information,
                SimpleCommand = SimpleCommands.SettingsCommand
            },
            new Command()
            {
                Word = "modlist",
                Description = "Displays the mods currently enforced, whitelisted and banned on the server.",
                Category = Command.CommandCategory.Information,
                SimpleCommand = SimpleCommands.ModListCommand
            },
            new Command()
            {
                Word = "reload",
                Description = "Reloads all server settings.",
                Category = Command.CommandCategory.Server_Administration,
                SimpleCommand = SimpleCommands.ReloadCommand
            },
            new Command()
            {
                Word = "status",
                Description = "Shows an overview of the server status.",
                Category = Command.CommandCategory.Information,
                SimpleCommand = SimpleCommands.StatusCommand
            },
            new Command()
            {
                Word = "eventlist",
                Description = "Displays a list of events to be used with 'invoke' and 'plague'.",
                Category = Command.CommandCategory.Information,
                SimpleCommand = SimpleCommands.EventListCommand
            },
            new Command()
            {
                Word = "chat",
                Description = "Recalls the cache of chat messages from the server.",
                Category = Command.CommandCategory.Player_Interaction,
                SimpleCommand = SimpleCommands.ChatCommand
            },
            new Command()
            {
                Word = "list",
                Description = "Displays a list of players.",
                Category = Command.CommandCategory.Information,
                SimpleCommand = SimpleCommands.ListCommand
            },
            new Command()
            {
                Word = "settlements",
                Description = "Displays settlement information.",
                Category = Command.CommandCategory.Information,
                SimpleCommand = SimpleCommands.SettlementsCommand
            },
            new Command()
            {
                Word = "banlist",
                Description = "Lists all banned players and their IPs.",
                Category = Command.CommandCategory.Information,
                SimpleCommand = SimpleCommands.BanListCommand
            },
            new Command()
            {
                Word = "adminlist",
                Description = "Lists all admins on the server.",
                Category = Command.CommandCategory.Information,
                SimpleCommand = SimpleCommands.AdminListCommand
            },
            new Command()
            {
                Word = "whitelist",
                Description = "Lists all whitelisted players on the server.",
                Category = Command.CommandCategory.Information,
                SimpleCommand = SimpleCommands.WhiteListCommand
            },
            new Command()
            {
                Word = "wipe",
                Description = "Delete all player data from the server, permanently.",
                Category = Command.CommandCategory.Server_Administration,
                SimpleCommand = SimpleCommands.WipeCommand
            },
            new Command()
            {
                Word = "clear",
                Description = "Clears this server console window.",
                Category = Command.CommandCategory.Information,
                SimpleCommand = SimpleCommands.ClearCommand
            },
            new Command()
            {
                Word = "exit",
                Description = "Closes the server.",
                Category = Command.CommandCategory.Server_Administration,
                SimpleCommand = SimpleCommands.ExitCommand
            },
            new Command()
            {
                Word = "say",
                Description = "Sends a chat message to the server.",
                Category = Command.CommandCategory.Player_Interaction,
                AdvancedCommand = AdvancedCommands.SayCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Message",
                        Description = "The message you would like to send in chat.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                    }
                }
            },
            new Command()
            {
                Word = "broadcast",
                Description = "Sends a notification to all connected players.",
                Category = Command.CommandCategory.Player_Interaction,
                AdvancedCommand = AdvancedCommands.BroadcastCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Message",
                        Description = "The message you would like to broadcast to all players.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                    }
                }
            },
            new Command()
            {
                Word = "notify",
                Description = "Sends a notification to a specific player.",
                Category = Command.CommandCategory.Player_Interaction,
                AdvancedCommand = AdvancedCommands.NotifyCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Player",
                        Description = "The player to whom you would like to send the notification.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.PlayerOnline
                        }
                    },
                    new Parameter()
                    {
                        Name = "Message",
                        Description = "The message you would like to send in chat.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                    }
                }
            },
            new Command()
            {
                Word = "invoke",
                Description = "Sends an event to a specific player (see 'eventlist').",
                Category = Command.CommandCategory.Player_Interaction,
                AdvancedCommand = AdvancedCommands.InvokeCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Player",
                        Description = "The player to whom you would like to send the notification.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.PlayerOnline
                        }
                    },
                    new Parameter()
                    {
                        Name = "Event",
                        Description = "The event you would like to send to the player.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.ValidEvent
                        }
                    }
                }
            },
            new Command()
            {
                Word = "plague",
                Description = "Sends an event to all connected players (see 'eventlist').",
                Category = Command.CommandCategory.Player_Interaction,
                AdvancedCommand = AdvancedCommands.PlagueCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Event",
                        Description = "The event you would like to send to the player.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.ValidEvent
                        }
                    }
                }

            },
            new Command()
            {
                Word = "player",
                Description = "Displays all data about a specific player.",
                Category = Command.CommandCategory.Information,
                AdvancedCommand = AdvancedCommands.PlayerDetailsCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Player",
                        Description = "The player of whom you would like to see information.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.PlayerOnline
                        }
                    }
                }
            },
            new Command()
            {
                Word = "faction",
                Description = "Displays information about a specific faction.",
                Category = Command.CommandCategory.Information,
                AdvancedCommand = AdvancedCommands.FactionDetailsCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Faction",
                        Description = "The faction of which you would like to see information.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                    }
                }
            },
            new Command()
            {
                Word = "kick",
                Description = "Kicks a specific player from the server.",
                Category = Command.CommandCategory.Server_Administration,
                AdvancedCommand = AdvancedCommands.KickCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Player",
                        Description = "The player who you would like to kick.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.PlayerOnline
                        }
                    }
                }
            },
            new Command()
            {
                Word = "ban",
                Description = "Bans a specific player from the server (by IP address).",
                Category = Command.CommandCategory.Server_Administration,
                AdvancedCommand = AdvancedCommands.BanCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Player",
                        Description = "The player who you would like to ban.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.PlayerOnline
                        }
                    }
                }
            },
            new Command()
            {
                Word = "pardon",
                Description = "Unbans a specific player from the server.",
                Category = Command.CommandCategory.Server_Administration,
                AdvancedCommand = AdvancedCommands.PardonCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Player",
                        Description = "The player who you would like to pardon.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                    }
                }
            },
            new Command()
            {
                Word = "promote",
                Description = "Promotes a specific player to an administrator.",
                Category = Command.CommandCategory.Server_Administration,
                AdvancedCommand = AdvancedCommands.PromoteCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Player",
                        Description = "The player who you would like to promote.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.PlayerOnline
                        }
                    }
                }
            },
            new Command()
            {
                Word = "demote",
                Description = "Revokes administrator permissions from a specific player.",
                Category = Command.CommandCategory.Server_Administration,
                AdvancedCommand = AdvancedCommands.DemoteCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Player",
                        Description = "The player who you would like to demote.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.PlayerOnline
                        }
                    }
                }
            },
            new Command()
            {
                Word = "giveitem",
                Description = "Gives a specific item to a specific player.",
                Category = Command.CommandCategory.Player_Interaction,
                AdvancedCommand = AdvancedCommands.GiveItemCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Player",
                        Description = "The player who you would like to give the item to.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.PlayerOnline
                        }
                    },
                    new Parameter()
                    {
                        Name = "Item",
                        Description = "The item which you would like to give to the player.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                    },
                    new Parameter()
                    {
                        Name = "Quantity",
                        Description = "The quantity of the item which you would like to give to the player.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                    },
                    new Parameter()
                    {
                        Name = "Quality",
                        Description = "The quality of the item which you would like to give to the player.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                    }
                }
            },
            new Command()
            {
                Word = "giveitemall",
                Description = "Gives a specific item to all connected players.",
                Category = Command.CommandCategory.Player_Interaction,
                AdvancedCommand = AdvancedCommands.GiveItemAllCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Item",
                        Description = "The item which you would like to give to all players.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                    },
                    new Parameter()
                    {
                        Name = "Quantity",
                        Description = "The quantity of the item which you would like to give to all players.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                    },
                    new Parameter()
                    {
                        Name = "Quality",
                        Description = "The quality of the item which you would like to give to all players.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                    }
                }
            },
            new Command()
            {
                Word = "protect",
                Description = "Protects a specific player from events until 'deprotect'ed.",
                Category = Command.CommandCategory.Server_Administration,
                AdvancedCommand = AdvancedCommands.ProtectCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Player",
                        Description = "The player who you would like to protect.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.PlayerOnline
                        }
                    }
                }
            },
            new Command()
            {
                Word = "deprotect",
                Description = "De'protect's a specific player from events.",
                Category = Command.CommandCategory.Server_Administration,
                AdvancedCommand = AdvancedCommands.DeprotectCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Player",
                        Description = "The player who you would like to deprotect.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.PlayerOnline
                        }
                    }
                }
            },
            new Command()
            {
                Word = "immunize",
                Description = "Temporarily protects a specific player from events.",
                Category = Command.CommandCategory.Server_Administration,
                AdvancedCommand = AdvancedCommands.ImmunizeCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Player",
                        Description = "The player who you would like to immunize.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.PlayerOnline
                        }
                    }
                }
            },
            new Command()
            {
                Word = "deimmunize",
                Description = "Demoves the temporary immunity granted by 'immunize'.",
                Category = Command.CommandCategory.Server_Administration,
                AdvancedCommand = AdvancedCommands.DeimmunizeCommand,
                Parameters = new HashSet<Parameter>()
                {
                    new Parameter()
                    {
                        Name = "Player",
                        Description = "The player who you would like to deimmunize.",
                        Rules = new HashSet<ParameterValidation.Rule>()
                        {
                            ParameterValidation.Rule.PlayerOnline
                        }
                    }
                }
            }
        };
    }
}