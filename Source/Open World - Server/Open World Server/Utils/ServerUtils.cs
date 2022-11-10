using System;
using System.Linq;
using System.Net;

namespace OpenWorldServer
{
    public static class ServerUtils
    {
        public static void CheckClientVersionRequirement()
        {
            ConsoleUtils.LogToConsole("Client Version Check", ConsoleUtils.ConsoleLogMode.Heading);

            try
            {
                string version;

                WebClient wc = new WebClient();
                version = wc.DownloadString("https://raw.githubusercontent.com/TastyLollipop/OpenWorld/main/Latest%20Versions%20Cache");
                version = version.Split('│')[2].Replace("- Latest Client Version: ", "");
                version = version.Remove(0, 1);
                version = version.Remove(version.Count() - 1, 1);

                Server.latestClientVersion = version;

                ConsoleUtils.LogToConsole("Listening For Version [" + Server.latestClientVersion + "]");
            }

            catch
            {
                ConsoleUtils.LogToConsole("Version Check Failed. This Is Not Dangerous", ConsoleUtils.ConsoleLogMode.Warning);
            }
        }

        public static void CheckServerVersion()
        {
            ConsoleUtils.LogToConsole("Settings Check", ConsoleUtils.ConsoleLogMode.Heading);

            string latestVersion = "";

            try
            {
                WebClient wc = new WebClient();
                latestVersion = wc.DownloadString("https://raw.githubusercontent.com/TastyLollipop/OpenWorld/main/Latest%20Versions%20Cache");
                latestVersion = latestVersion.Split('│')[1].Replace("- Latest Server Version: ", "");
                latestVersion = latestVersion.Remove(0, 1);
                latestVersion = latestVersion.Remove(latestVersion.Count() - 1, 1);
            }

            catch
            {
                Console.ForegroundColor = ConsoleColor.White;
                ConsoleUtils.LogToConsole("Version Check Failed. This is not dangerous");
                Console.ForegroundColor = ConsoleColor.White;
            }

            if (Server.serverVersion == latestVersion) ConsoleUtils.LogToConsole("Running Latest Version");
            else ConsoleUtils.LogToConsole("Running Outdated Or Unstable version. Please Update From Github At Earliest Convenience To Prevent Errors");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}