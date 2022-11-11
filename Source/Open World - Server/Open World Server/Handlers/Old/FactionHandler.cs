using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using OpenWorldServer.Services;

namespace OpenWorldServer.Handlers.Old
{
    public static class FactionHandler
    {
        public enum MemberRank { Member, Moderator, Leader }

        public static void CheckFactions()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Factions Check", ConsoleUtils.ConsoleLogMode.Heading);
            Console.ForegroundColor = ConsoleColor.White;

            if (!Directory.Exists(PathProvider.FactionsFolderPath))
            {
                Directory.CreateDirectory(PathProvider.FactionsFolderPath);
                ConsoleUtils.LogToConsole("No Factions Folder Found, Generating");
            }

            else
            {
                string[] factionFiles = Directory.GetFiles(PathProvider.FactionsFolderPath);

                if (factionFiles.Length == 0)
                {
                    ConsoleUtils.LogToConsole("No Factions Found, Ignoring");
                }

                else LoadFactions(factionFiles);
            }
        }

        public static void CreateFaction(string factionName, PlayerClient factionLeader)
        {
            FactionOld newFaction = new FactionOld()
            {
                name = factionName,
                wealth = 0,
                members = new Dictionary<PlayerClient, MemberRank>() { { factionLeader, MemberRank.Leader } }
            };
            SaveFaction(newFaction);

            factionLeader.Account.Faction = newFaction;

            PlayerClient clientToSave = Server.savedClients.Find(fetch => fetch.Account.Username == factionLeader.Account.Username);
            clientToSave.Account.Faction = newFaction;
            StaticProxy.playerManager.AccountsHandler.SaveAccount(clientToSave);

            Networking.SendData(factionLeader, "FactionManagement│Created");

            Networking.SendData(factionLeader, GetFactionDetails(factionLeader));
        }

        public static void SaveFaction(FactionOld factionToSave)
        {
            string factionSavePath = PathProvider.FactionsFolderPath + Path.DirectorySeparatorChar + factionToSave.name + ".bin";

            if (factionToSave.members.Count() > 1)
            {
                var orderedDictionary = factionToSave.members.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                factionToSave.members = orderedDictionary;
            }

            Stream s = File.OpenWrite(factionSavePath);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(s, factionToSave);

            s.Flush();
            s.Close();
            s.Dispose();

            if (!Server.savedFactions.Contains(factionToSave)) Server.savedFactions.Add(factionToSave);
        }

        public static void LoadFactions(string[] factionFiles)
        {
            int failedToLoadFactions = 0;

            foreach (string faction in factionFiles)
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    FileStream s = File.Open(faction, FileMode.Open);
                    object obj = formatter.Deserialize(s);
                    FactionOld factionToLoad = (FactionOld)obj;

                    s.Flush();
                    s.Close();
                    s.Dispose();

                    if (factionToLoad.members.Count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        ConsoleUtils.LogToConsole("Faction Had 0 Members, Removing");
                        Console.ForegroundColor = ConsoleColor.White;

                        DisbandFaction(factionToLoad);
                        continue;
                    }

                    FactionOld factionToFetch = Server.savedFactions.Find(fetch => fetch.name == factionToLoad.name);
                    if (factionToFetch == null) Server.savedFactions.Add(factionToLoad);
                }
                catch { failedToLoadFactions++; }
            }

            ConsoleUtils.LogToConsole("Loaded [" + Server.savedFactions.Count() + "] Factions");

            if (failedToLoadFactions > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleUtils.LogToConsole("Failed to load [" + failedToLoadFactions + "] Factions");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void DisbandFaction(FactionOld factionToDisband)
        {
            Server.savedFactions.Remove(factionToDisband);

            string factionSavePath = PathProvider.FactionsFolderPath + Path.DirectorySeparatorChar + factionToDisband.name + ".bin";
            File.Delete(factionSavePath);
        }

        public static string GetFactionDetails(PlayerClient client)
        {
            string dataToSend = "FactionManagement│Details│";

            if (client.Account.Faction == null) return dataToSend;

            else
            {
                FactionOld factionToCheck = Server.savedFactions.Find(fetch => fetch.name == client.Account.Faction.name);

                dataToSend += factionToCheck.name + "│";

                Dictionary<PlayerClient, MemberRank> members = factionToCheck.members;
                foreach (KeyValuePair<PlayerClient, MemberRank> member in members)
                {
                    dataToSend += member.Key.Account.Username + ":" + (int)member.Value + "»";
                }

                return dataToSend;
            }
        }

        public static void AddMember(FactionOld faction, PlayerClient memberToAdd)
        {
            faction.members.Add(memberToAdd, MemberRank.Member);
            SaveFaction(faction);

            PlayerClient connected = StaticProxy.playerManager.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == memberToAdd.Account.Username);
            if (connected != null)
            {
                connected.Account.Faction = faction;
                //Don't need to send the data here since it's gonna be done down bellow
                //Networking.SendData(connected, GetFactionDetails(connected));
            }

            PlayerClient saved = Server.savedClients.Find(fetch => fetch.Account.Username == memberToAdd.Account.Username);
            if (saved != null)
            {
                saved.Account.Faction = faction;
                StaticProxy.playerManager.AccountsHandler.SaveAccount(saved);
            }

            UpdateAllPlayerDetailsInFaction(faction);
        }

        public static void RemoveMember(FactionOld faction, PlayerClient memberToRemove)
        {
            foreach (KeyValuePair<PlayerClient, MemberRank> pair in faction.members)
            {
                if (pair.Key.Account.Username == memberToRemove.Account.Username)
                {
                    faction.members.Remove(pair.Key);
                    break;
                }
            }

            PlayerClient connected = StaticProxy.playerManager.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == memberToRemove.Account.Username);
            if (connected != null)
            {
                connected.Account.Faction = null;
                Networking.SendData(connected, GetFactionDetails(connected));
            }

            PlayerClient saved = Server.savedClients.Find(fetch => fetch.Account.Username == memberToRemove.Account.Username);
            if (saved != null)
            {
                saved.Account.Faction = null;
                StaticProxy.playerManager.AccountsHandler.SaveAccount(saved);
            }

            if (faction.members.Count > 0)
            {
                SaveFaction(faction);
                UpdateAllPlayerDetailsInFaction(faction);
            }
            else DisbandFaction(faction);
        }

        public static void PurgeFaction(FactionOld faction)
        {
            PlayerClient[] dummyfactionMembers = faction.members.Keys.ToArray();

            foreach (PlayerClient dummy in dummyfactionMembers)
            {
                PlayerClient connected = StaticProxy.playerManager.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == dummy.Account.Username);
                if (connected != null)
                {
                    connected.Account.Faction = null;
                    Networking.SendData(connected, GetFactionDetails(connected));
                }

                PlayerClient saved = Server.savedClients.Find(fetch => fetch.Account.Username == dummy.Account.Username);
                if (saved != null)
                {
                    saved.Account.Faction = null;
                    StaticProxy.playerManager.AccountsHandler.SaveAccount(saved);
                }
            }

            DisbandFaction(faction);
        }

        public static void PromoteMember(FactionOld faction, PlayerClient memberToPromote)
        {
            PlayerClient toPromote = null;
            MemberRank previousRank = 0;

            foreach (KeyValuePair<PlayerClient, MemberRank> pair in faction.members)
            {
                if (pair.Key.Account.Username == memberToPromote.Account.Username)
                {
                    toPromote = pair.Key;
                    previousRank = pair.Value;
                    break;
                }
            }

            if (previousRank > 0) return;

            faction.members.Remove(toPromote);

            faction.members.Add(memberToPromote, MemberRank.Moderator);

            SaveFaction(faction);
            UpdateAllPlayerDetailsInFaction(faction);
        }

        public static void DemoteMember(FactionOld faction, PlayerClient memberToPromote)
        {
            PlayerClient toDemote = null;
            MemberRank previousRank = 0;

            foreach (KeyValuePair<PlayerClient, MemberRank> pair in faction.members)
            {
                if (pair.Key.Account.Username == memberToPromote.Account.Username)
                {
                    toDemote = pair.Key;
                    previousRank = pair.Value;
                    break;
                }
            }

            if (previousRank == 0) return;

            faction.members.Remove(toDemote);

            faction.members.Add(memberToPromote, MemberRank.Member);

            SaveFaction(faction);
            UpdateAllPlayerDetailsInFaction(faction);
        }

        public static void UpdateAllPlayerDetailsInFaction(FactionOld faction)
        {
            PlayerClient[] dummyfactionMembers = faction.members.Keys.ToArray();

            foreach (PlayerClient dummy in dummyfactionMembers)
            {
                PlayerClient connected = StaticProxy.playerManager.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == dummy.Account.Username);
                if (connected != null)
                {
                    Networking.SendData(connected, GetFactionDetails(connected));
                }
            }
        }

        public static MemberRank GetMemberPowers(FactionOld faction, PlayerClient memberToCheck)
        {
            Dictionary<PlayerClient, MemberRank> members = faction.members;
            foreach (KeyValuePair<PlayerClient, MemberRank> pair in members)
            {
                if (pair.Key.Account.Username == memberToCheck.Account.Username)
                {
                    return pair.Value;
                }
            }

            return MemberRank.Member;
        }
    }
}