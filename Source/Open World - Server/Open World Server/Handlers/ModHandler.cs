using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenWorldServer
{
    public static class ModHandler
    {
        public static void CheckMods(bool newLine)
        {
            if (newLine) Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Mods Check:");
            Console.ForegroundColor = ConsoleColor.White;

            CheckEnforcedMods();
            CheckWhitelistedMods();
            CheckBlacklistedMods();
        }

        public static void CheckEnforcedMods()
        {
            if (!Directory.Exists(OpenWorldServer.enforcedModsFolderPath))
            {
                Directory.CreateDirectory(OpenWorldServer.enforcedModsFolderPath);
                ConsoleUtils.LogToConsole("No Enforced Mods Folder Found, Generating");
            }

            else
            {
                string[] modFolders = Directory.GetDirectories(OpenWorldServer.enforcedModsFolderPath);

                if (modFolders.Length == 0)
                {
                    ConsoleUtils.LogToConsole("No Enforced Mods Found, Ignoring");
                    return;
                }

                else LoadMods(modFolders, 0);
            }
        }

        public static void CheckWhitelistedMods()
        {
            if (!Directory.Exists(OpenWorldServer.whitelistedModsFolderPath))
            {
                Directory.CreateDirectory(OpenWorldServer.whitelistedModsFolderPath);
                ConsoleUtils.LogToConsole("No Whitelisted Mods Folder Found, Generating");
            }

            else
            {
                string[] modFolders = Directory.GetDirectories(OpenWorldServer.whitelistedModsFolderPath);

                if (modFolders.Length == 0) ConsoleUtils.LogToConsole("No Whitelisted Mods Found, Ignoring");

                else LoadMods(modFolders, 1);
            }
        }

        public static void CheckBlacklistedMods()
        {
            if (!Directory.Exists(OpenWorldServer.blacklistedModsFolderPath))
            {
                Directory.CreateDirectory(OpenWorldServer.blacklistedModsFolderPath);
                ConsoleUtils.LogToConsole("No Blacklisted Mods Folder Found, Generating");
            }

            else
            {
                string[] modFolders = Directory.GetDirectories(OpenWorldServer.blacklistedModsFolderPath);

                if (modFolders.Length == 0) ConsoleUtils.LogToConsole("No Blacklisted Mods Found, Ignoring");

                else LoadMods(modFolders, 2);
            }
        }

        private static void LoadMods(string[] modFolders, int modType)
        {
            int failedToLoadMods = 0;
            List<string> modList = new List<string>();

            if (modType == 0) OpenWorldServer.enforcedMods.Clear();
            else if (modType == 1) OpenWorldServer.whitelistedMods.Clear();
            else if (modType == 2) OpenWorldServer.blacklistedMods.Clear();

            foreach (string modFolder in modFolders)
            {
                try
                {
                    string aboutFilePath = modFolder + Path.DirectorySeparatorChar + "About" + Path.DirectorySeparatorChar + "About.xml";
                    string[] aboutLines = File.ReadAllLines(aboutFilePath);

                    foreach (string line in aboutLines)
                    {
                        if (line.Contains("<name>") && line.Contains("</name>"))
                        {
                            string modName = line;

                            string purgeString = modName.Split('<')[0];
                            modName = modName.Remove(0, purgeString.Count());

                            modName = modName.Replace("<name>", "");
                            modName = modName.Replace("</name>", "");

                            if (modName.Contains("")) modName = modName.Replace("&amp", "&");
                            if (modName.Contains("")) modName = modName.Replace("&quot", "&");
                            if (modName.Contains("")) modName = modName.Replace("&lt", "&");

                            modList.Add(modName);
                            break;
                        }
                    }
                }

                catch { failedToLoadMods++; }
            }

            modList.Sort();
            if (modType == 0)
            {
                OpenWorldServer.enforcedMods = modList;
                ConsoleUtils.LogToConsole("Loaded [" + OpenWorldServer.enforcedMods.Count() + "] Enforced Mods");
            }
            else if (modType == 1)
            {
                OpenWorldServer.whitelistedMods = modList;
                ConsoleUtils.LogToConsole("Loaded [" + OpenWorldServer.whitelistedMods.Count() + "] Whitelisted Mods");
            }
            else if (modType == 2)
            {
                OpenWorldServer.blacklistedMods = modList;
                ConsoleUtils.LogToConsole("Loaded [" + OpenWorldServer.blacklistedMods.Count() + "] Blacklisted Mods");
            }

            if (failedToLoadMods > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleUtils.LogToConsole("Failed to load [" + failedToLoadMods + "] Mods");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
