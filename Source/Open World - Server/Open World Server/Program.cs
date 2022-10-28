using System.Globalization;
using System.IO;
using OpenWorldServer.Data;
using OpenWorldServer.Migrations;
using OpenWorldServer.Services;

namespace OpenWorldServer
{
    public static class Program
    {
        private static ServerConfig serverConfig;

        public static void Main(string[] args)
        {
            ConsoleUtils.LogTitleToConsole("Starting Server");
            PathProvider.EnsureDirectories();

            SetCulture();
            serverConfig = ServerUtils.LoadServerConfig(PathProvider.ConfigFile);

            MigrationService.CreateAndMigrateAll(serverConfig); // Temp Migration Helper
            return;

            SetPaths();

            ServerUtils.CheckServerVersion();

            Server.Run();
        }

        private static void SetCulture()
        {
            // We use the US Culture so we don't need to watch when parsing the values with decimal points
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

        public static void SetPaths()
        {
            Server.enforcedModsFolderPath = Path.Combine(PathProvider.MainFolderPath, "Enforced Mods");
            Server.whitelistedModsFolderPath = Path.Combine(PathProvider.MainFolderPath, "Whitelisted Mods");
            Server.blacklistedModsFolderPath = Path.Combine(PathProvider.MainFolderPath, "Blacklisted Mods");
            Server.whitelistedUsersPath = Path.Combine(PathProvider.MainFolderPath, "Whitelisted Players.txt");
        }

    }
}
