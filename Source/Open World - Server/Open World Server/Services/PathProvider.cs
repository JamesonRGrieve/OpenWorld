using System.IO;
using System.Reflection;

namespace OpenWorldServer.Services
{
    internal static class PathProvider
    {
        internal static readonly string MainFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static readonly string LogsFolderPath = Path.Combine(MainFolderPath, "Logs");
        internal static readonly string PlayersFolderPath = Path.Combine(MainFolderPath, "Players");
        internal static readonly string FactionsFolderPath = Path.Combine(MainFolderPath, "Factions");
        //internal static readonly string PlayersFolderPath = Path.Combine(MainFolderPath, "Players");
        //internal static readonly string PlayersFolderPath = Path.Combine(MainFolderPath, "Players");
        //internal static readonly string PlayersFolderPath = Path.Combine(MainFolderPath, "Players");

        internal static readonly string ConfigFile = Path.Combine(MainFolderPath, "config.json");
        internal static readonly string PlayerWhitelistFile = Path.Combine(MainFolderPath, "whitelist.json");

        public static void EnsureDirectories()
        {
            Directory.CreateDirectory(LogsFolderPath);
            Directory.CreateDirectory(PlayersFolderPath);
            Directory.CreateDirectory(FactionsFolderPath);
        }
    }
}
