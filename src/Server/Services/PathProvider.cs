using System.IO;
using System.Reflection;

namespace OpenWorld.Server.Services
{
    internal static class PathProvider
    {
        internal static readonly string MainFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static readonly string LogsFolderPath = Path.Combine(MainFolderPath, "Logs");
        internal static readonly string PlayersFolderPath = Path.Combine(MainFolderPath, "Players");
        internal static readonly string FactionsFolderPath = Path.Combine(MainFolderPath, "Factions");
        internal static readonly string BaseModsFolderPath = Path.Combine(MainFolderPath, "Mods");
        internal static readonly string RequiredModsFolderPath = Path.Combine(BaseModsFolderPath, "Enforced");
        internal static readonly string WhitelistedModsFolderPath = Path.Combine(BaseModsFolderPath, "Whitelisted");
        internal static readonly string BlacklistedModsFolderPath = Path.Combine(BaseModsFolderPath, "Blacklisted");

        internal static readonly string ConfigFile = Path.Combine(MainFolderPath, "config.json");
        internal static readonly string PlayerWhitelistFile = Path.Combine(MainFolderPath, "whitelist.json");
        internal static readonly string BannedPlayersFile = Path.Combine(MainFolderPath, "banlist.json");

        internal static void EnsureDirectories()
        {
            Directory.CreateDirectory(LogsFolderPath);
            Directory.CreateDirectory(PlayersFolderPath);
            Directory.CreateDirectory(FactionsFolderPath);
            Directory.CreateDirectory(BaseModsFolderPath);
            Directory.CreateDirectory(RequiredModsFolderPath);
            Directory.CreateDirectory(WhitelistedModsFolderPath);
            Directory.CreateDirectory(BlacklistedModsFolderPath);
        }
    }
}
