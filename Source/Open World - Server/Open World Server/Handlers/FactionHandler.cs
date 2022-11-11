using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OpenWorld.Shared.Enums;
using OpenWorld.Shared.Networking.Packets.Faction;
using OpenWorldServer.Data;
using OpenWorldServer.Data.Factions;
using OpenWorldServer.Manager;
using OpenWorldServer.Services;
using OpenWorldServer.Utils;

namespace OpenWorldServer.Handlers
{
    public class FactionHandler
    {
        private readonly ServerConfig serverConfig;
        private readonly PlayerManager playerManager;

        /// <summary>
        /// Copy of Factions as ReadOnlyCollection
        /// </summary>
        public ReadOnlyCollection<Faction> Factions => this.factions.ToList().AsReadOnly();

        private List<Faction> factions = new List<Faction>();

        public FactionHandler(ServerConfig serverConfig, PlayerManager playerManager)
        {
            this.serverConfig = serverConfig;
            this.playerManager = playerManager;
            this.LoadFactions();
        }

        private void LoadFactions()
        {
            ConsoleUtils.LogTitleToConsole("Loading Factions");
            foreach (var factionFile in Directory.GetFiles(PathProvider.FactionsFolderPath, "*.json"))
            {
                this.LoadFaction(factionFile);
            }

            ConsoleUtils.LogToConsole($"Loaded Factions - {this.factions.Count} Entries", ConsoleUtils.ConsoleLogMode.Info);
        }

        private void LoadFaction(string factionFile)
        {
            try
            {
                var faction = JsonDataHelper.Load<Faction>(factionFile);
                if (faction.Members.Count == 0)
                {
                    this.DeleteFaction(faction.Name, $"Faction [{faction.Name}] has 0 Members, removing");
                    return;
                }

                this.factions.Add(faction);
            }
            catch (Exception ex)
            {
                ConsoleUtils.LogToConsole($"Error loading Faction from '{factionFile}'. Exception:", ConsoleUtils.ConsoleLogMode.Error);
                ConsoleUtils.LogToConsole(ex.Message, ConsoleUtils.ConsoleLogMode.Error);
            }
        }

        public bool SaveFaction(Faction faction, bool saveOnly = false)
        {
            try
            {
                ConsoleUtils.LogToConsole($"Saving Faction [{faction.Name}]", ConsoleUtils.ConsoleLogMode.Done);
                JsonDataHelper.Save(faction, this.GetFactionFilePath(faction.Name));
                if (!saveOnly && !this.Factions.Contains(faction))
                {
                    this.factions.Add(faction);
                }

                return true;
            }
            catch (Exception ex)
            {
                ConsoleUtils.LogToConsole($"Error saving Faction [{faction.Name}]", ConsoleUtils.ConsoleLogMode.Error);
                ConsoleUtils.LogToConsole(ex.Message, ConsoleUtils.ConsoleLogMode.Error);
                return false;
            }
        }

        public void CreateFaction(string factionName, PlayerClient owner)
        {
            var faction = new Faction()
            {
                Name = factionName,
            };

            faction.Members.Add(owner.Account.Id, FactionRank.Leader);
            owner.Account.FactionId = faction.Id;

            this.SaveFaction(faction);
            this.playerManager.AccountsHandler.SaveAccount(owner);
            owner.SendData(new FactionCreatedPacket());
            owner.SendData(this.GetFactionDetailsPacket(faction));
        }

        public void PurgeFaction(Faction faction)
        {
            foreach (var kvp in faction.Members.ToList())
            {
                var memberId = kvp.Key;
                var client = this.playerManager.GetClient(memberId);
                var member = client != null ? client.Account : this.playerManager.AccountsHandler.GetAccount(memberId);
                if (member != null)
                {
                    member.FactionId = null;
                    this.playerManager.AccountsHandler.SaveAccount(member);
                }

                client?.SendData(new FactionDetailsPacket(null));
            }

            faction.Members.Clear();
            this.DeleteFaction(faction.Name, $"Faction [{faction.Name}] has 0 Members, removing");
            this.factions.Remove(faction);
        }

        public FactionRank GetMemberRank(Guid memberId)
        {
            var member = this.playerManager.AccountsHandler.GetAccount(memberId);
            if (member != null)
            {
                this.GetMemberRank((Guid)member.FactionId, memberId);
            }

            return FactionRank.NotMember;
        }

        public FactionRank GetMemberRank(Guid factionId, Guid memberId)
            => this.GetMemberRank(this.GetFaction(factionId), memberId);

        public FactionRank GetMemberRank(Faction faction, Guid memberId)
        {
            if (faction != null && faction.Members.ContainsKey(memberId))
            {
                return faction.Members[memberId];
            }

            return FactionRank.NotMember;
        }

        public void AddMember(Guid factionId, Guid memberId, FactionRank rank)
            => this.AddMember(this.GetFaction(factionId), this.playerManager.AccountsHandler.GetAccount(memberId), rank);

        private void AddMember(Faction faction, Account member, FactionRank rank)
        {
            if (faction == null || member == null)
            {
                return;
            }

            member.FactionId = faction.Id;
            faction.Members[member.Id] = rank;

            this.playerManager.AccountsHandler.SaveAccount(member);
            this.SaveFaction(faction);
            this.NotifyFactionMembers(faction);

            ConsoleUtils.LogToConsole($"Added [{member.Username}] to Faction [{faction.Name}]");
        }

        public void RemoveMember(Faction faction, Guid memberId)
        {
            if (faction == null)
            {
                return;
            }

            if (faction.Members.ContainsKey(memberId))
            {
                faction.Members.Remove(memberId);
            }

            var client = this.playerManager.GetClient(memberId);
            client?.SendData(new FactionDetailsPacket(null));

            var member = client != null ? client.Account : this.playerManager.AccountsHandler.GetAccount(memberId);
            if (member != null)
            {
                member.FactionId = null;
                this.playerManager.AccountsHandler.SaveAccount(member);
            }

            if (faction.Members.Count > 0)
            {
                this.NotifyFactionMembers(faction);
                this.SaveFaction(faction);
            }
            else
            {
                this.DeleteFaction(faction.Name, $"Faction [{faction.Name}] has 0 Members, removing");
                this.factions.Remove(faction);
            }
        }

        private void NotifyFactionMembers(Faction faction)
        {
            var clients = this.playerManager.ConnectedClients;
            var packet = this.GetFactionDetailsPacket(faction);
            foreach (var client in faction.Members.ToList().Select(m => this.playerManager.GetClient(m.Key)))
            {
                if (client.IsLoggedIn)
                {
                    client.SendData(packet);
                }
            }
        }

        public FactionDetailsPacket GetFactionDetailsPacket(Guid? factionId)
        {
            if (factionId.HasValue)
            {
                var faction = this.GetFaction(factionId.Value);
                if (faction != null)
                {
                    return this.GetFactionDetailsPacket(faction);
                }
            }

            return new FactionDetailsPacket(null);
        }

        public FactionDetailsPacket GetFactionDetailsPacket(Faction faction)
            => new FactionDetailsPacket(faction.Members.Select(m => (this.playerManager.AccountsHandler.GetAccount(m.Key)?.Username, (FactionRank)m.Value)));

        public Faction GetFaction(Guid id) => this.Factions.FirstOrDefault(a => a.Id == id);

        private string SanitizedName(string name) => name.Replace(' ', '-');

        private string GetFactionFilePath(string factionName) => Path.Combine(PathProvider.FactionsFolderPath, $"{this.SanitizedName(factionName)}.json");

        private void DeleteFaction(string factionName, string reason)
        {
            ConsoleUtils.LogToConsole(reason, ConsoleUtils.ConsoleLogMode.Info);
            File.Delete(this.GetFactionFilePath(factionName));
        }
    }
}
