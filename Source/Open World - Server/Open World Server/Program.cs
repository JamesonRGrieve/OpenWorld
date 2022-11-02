﻿using System;
using System.Globalization;
using System.IO;
using OpenWorldServer.Data;
using OpenWorldServer.Services;
using OpenWorldServer.Utils;

namespace OpenWorldServer
{
    public static class Program
    {
        private static ServerConfig serverConfig;
        private static Server server;

        public static void Main(string[] args)
        {
            ConsoleUtils.LogTitleToConsole("Starting Server");
            PathProvider.EnsureDirectories();

            SetCulture();
            serverConfig = LoadServerConfig(PathProvider.ConfigFile);

            MigrationService.CreateAndMigrateAll(serverConfig); // Temp Migration Helper
            server = new Server(serverConfig);

            ServerUtils.CheckServerVersion();

            server.Run();
        }

        private static void SetCulture()
        {
            // We use the US Culture so we don't need to watch out when parsing the values with decimal points
            // Better practice would be to parse the values with the us culture set instead of changeing the CultureInfo.
            ConsoleUtils.LogTitleToConsole("Updating Culture Info");
            ConsoleUtils.LogToConsole("Old Culture Info: [" + CultureInfo.CurrentCulture + "]");

            var usCulture = new CultureInfo("en-US", false);
            CultureInfo.CurrentCulture = usCulture;
            CultureInfo.CurrentUICulture = usCulture;
            CultureInfo.DefaultThreadCurrentCulture = usCulture;
            CultureInfo.DefaultThreadCurrentUICulture = usCulture;

            ConsoleUtils.LogToConsole("New Culture Info: [" + CultureInfo.CurrentCulture + "]");
        }

        private static ServerConfig LoadServerConfig(string filePath)
        {
            var config = new ServerConfig();

            Console.WriteLine();
            ConsoleUtils.LogToConsole("Loading Server Settings", ConsoleColor.Green);

            if (File.Exists(filePath))
            {
                try
                {
                    config = JsonDataHelper.Load<ServerConfig>(filePath);
                }
                catch (Exception ex)
                {
                    // Possible error would be incorrect data
                    ConsoleUtils.LogToConsole("Error while loading Server Settings:", ConsoleColor.Red);
                    ConsoleUtils.LogToConsole(ex.Message, ConsoleColor.Red);

                    return null;
                }
            }
            else
            {
                ConsoleUtils.LogToConsole("No Server Settings File found, generating new one", ConsoleColor.Yellow);
                JsonDataHelper.Save(config, filePath);
            }

            ConsoleUtils.LogToConsole("Loaded Server Settings");
            return config;
        }
    }
}
