using System.IO;
using System.Reflection;

namespace OpenWorldServer.Services
{
    internal class PathProvider
    {
        internal static readonly string MainFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static readonly string ConfigFile = Path.Combine(MainFolderPath, "config.json");
    }
}
