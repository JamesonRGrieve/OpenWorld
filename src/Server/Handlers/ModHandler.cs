﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using OpenWorld.Server.Data;
using OpenWorld.Server.Services;

namespace OpenWorld.Server.Handlers
{
    public class ModHandler
    {
        private readonly ServerConfig serverConfig;
        private readonly XmlSerializer xmlSerializer;

        public ReadOnlySpan<ModMetaData> RequiredMods => this.requiredMods;

        public ReadOnlySpan<ModMetaData> WhitelistedMods => this.whitelistedMods;

        public ReadOnlySpan<ModMetaData> BlacklistedMods => this.blacklistedMods;

        private ModMetaData[] requiredMods;
        private ModMetaData[] whitelistedMods;
        private ModMetaData[] blacklistedMods;

        public ModHandler(ServerConfig serverConfig)
        {
            this.serverConfig = serverConfig;
            this.xmlSerializer = new XmlSerializer(typeof(ModMetaData));

            this.ReloadModFolders();
        }

        public void ReloadModFolders()
        {
            ConsoleUtils.LogTitleToConsole("Reloading Mods");
            this.requiredMods = this.GetMods(PathProvider.RequiredModsFolderPath);
            this.whitelistedMods = this.GetMods(PathProvider.WhitelistedModsFolderPath);
            this.blacklistedMods = this.GetMods(PathProvider.BlacklistedModsFolderPath);
            ConsoleUtils.LogToConsole($"Loaded {this.requiredMods.Length} Required Mods", ConsoleUtils.ConsoleLogMode.Info);
            ConsoleUtils.LogToConsole($"Loaded {this.whitelistedMods.Length} Whitelisted Mods", ConsoleUtils.ConsoleLogMode.Info);
            ConsoleUtils.LogToConsole($"Loaded {this.blacklistedMods.Length} Blacklisted Mods", ConsoleUtils.ConsoleLogMode.Info);
        }

        public bool IsModEnforced(string modName) => this.requiredMods.Any(mmd => mmd.Name == modName);

        public bool IsModWhitelisted(string modName) => this.whitelistedMods.Any(mmd => mmd.Name == modName);

        public bool IsModBlacklisted(string modName) => this.blacklistedMods.Any(mmd => mmd.Name == modName);

        private ModMetaData[] GetMods(string path)
        {
            const string aboutFileName = "About.xml";
            const string aboutDirName = "About";

            var mods = new List<ModMetaData>();
            foreach (var folder in Directory.GetDirectories(path))
            {
                var aboutDir = Path.Combine(folder, aboutDirName);
                // we look it up here so we can give a proper warning if for example there are more About Files somewhere hidden in the folder
                var foundAboutFiles = Directory.GetFiles(aboutDir, aboutFileName, SearchOption.AllDirectories);
                if (foundAboutFiles.Length > 1)
                {
                    // This could be mean someone wanted to hide a mod
                    ConsoleUtils.LogToConsole($"!! WARNING !! Found more than one '{aboutFileName}' in '{aboutDir}'. Skipping...", ConsoleUtils.ConsoleLogMode.Warning);
                }
                else if (foundAboutFiles.Length == 1)
                {
                    try
                    {
                        using (var fo = File.OpenRead(foundAboutFiles.First()))
                        {
                            var mod = (ModMetaData)this.xmlSerializer.Deserialize(fo);
                            mods.Add(mod);
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleUtils.LogToConsole($"Error reading Mod '{folder}'. Reason: {ex.Message}", ConsoleUtils.ConsoleLogMode.Error);
                    }
                }
            }

            return mods.ToArray();
        }
    }
}
