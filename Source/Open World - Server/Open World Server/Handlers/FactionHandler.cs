using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using OpenWorldServer.Services;

namespace OpenWorldServer
{
    public static class FactionHandler
    {
        public enum MemberRank { Member, Moderator, Leader }

        public static void CheckFactions(bool newLine)
        {
            if (newLine) Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Green;
            ConsoleUtils.LogToConsole("Factions Check:");
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
                    Console.WriteLine("");
                }

                else LoadFactions(factionFiles);
            }
        }

        public static void CreateFaction(string factionName, ServerClient factionLeader)
        {
            Faction newFaction = new Faction();
            newFaction.name = factionName;
            newFaction.wealth = 0;
            newFaction.members.Add(factionLeader, MemberRank.Leader);
            SaveFaction(newFaction);

            factionLeader.PlayerData.Faction = newFaction;

            ServerClient clientToSave = Server.savedClients.Find(fetch => fetch.PlayerData.Username == factionLeader.PlayerData.Username);
            clientToSave.PlayerData.Faction = newFaction;
            StaticProxy.playerHandler.SavePlayerData(clientToSave);

            Networking.SendData(factionLeader, "FactionManagement│Created");

            Thread.Sleep(100);

            Networking.SendData(factionLeader, GetFactionDetails(factionLeader));
        }

        public static void SaveFaction(Faction factionToSave)
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
                    Faction factionToLoad = (Faction)obj;

                    s.Flush();
                    s.Close();
                    s.Dispose();

                    if (!Server.savedFactions.Contains(factionToLoad)) Server.savedFactions.Add(factionToLoad);
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

        public static void DisbandFaction(Faction factionToDisband)
        {
            Server.savedFactions.Remove(factionToDisband);

            string factionSavePath = PathProvider.FactionsFolderPath + Path.DirectorySeparatorChar + factionToDisband.name + ".bin";
            File.Delete(factionSavePath);
        }

        public static string GetFactionDetails(ServerClient client)
        {
            string dataToSend = "FactionManagement│Details│";

            if (client.PlayerData.Faction == null) return dataToSend;

            else
            {
                Faction factionToCheck = Server.savedFactions.Find(fetch => fetch.name == client.PlayerData.Faction.name);

                dataToSend += factionToCheck.name + "│";

                foreach (KeyValuePair<ServerClient, MemberRank> member in factionToCheck.members)
                {
                    dataToSend += member.Key.PlayerData.Username + ":" + (int)member.Value + "»";
                }

                return dataToSend;
            }
        }

        public static void AddMember(Faction faction, ServerClient memberToAdd)
        {
            faction.members.Add(memberToAdd, MemberRank.Member);
            SaveFaction(faction);

            ServerClient connected = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == memberToAdd.PlayerData.Username);
            if (connected != null)
            {
                connected.PlayerData.Faction = faction;
                //Don't need to send the data here since it's gonna be done down bellow
                //Networking.SendData(connected, GetFactionDetails(connected));
            }

            ServerClient saved = Server.savedClients.Find(fetch => fetch.PlayerData.Username == memberToAdd.PlayerData.Username);
            if (saved != null)
            {
                saved.PlayerData.Faction = faction;
                StaticProxy.playerHandler.SavePlayerData(saved);
            }

            UpdateAllPlayerDetailsInFaction(faction);
        }

        public static void RemoveMember(Faction faction, ServerClient memberToRemove)
        {
            foreach (KeyValuePair<ServerClient, MemberRank> pair in faction.members)
            {
                if (pair.Key.PlayerData.Username == memberToRemove.PlayerData.Username)
                {
                    faction.members.Remove(pair.Key);
                    break;
                }
            }

            ServerClient connected = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == memberToRemove.PlayerData.Username);
            if (connected != null)
            {
                connected.PlayerData.Faction = null;
                Networking.SendData(connected, GetFactionDetails(connected));
            }

            ServerClient saved = Server.savedClients.Find(fetch => fetch.PlayerData.Username == memberToRemove.PlayerData.Username);
            if (saved != null)
            {
                saved.PlayerData.Faction = null;
                StaticProxy.playerHandler.SavePlayerData(saved);
            }

            if (faction.members.Count > 0)
            {
                SaveFaction(faction);
                UpdateAllPlayerDetailsInFaction(faction);
            }
            else DisbandFaction(faction);
        }

        public static void PurgeFaction(Faction faction)
        {
            ServerClient[] dummyfactionMembers = faction.members.Keys.ToArray();

            foreach (ServerClient dummy in dummyfactionMembers)
            {
                ServerClient connected = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == dummy.PlayerData.Username);
                if (connected != null)
                {
                    connected.PlayerData.Faction = null;
                    Networking.SendData(connected, GetFactionDetails(connected));
                }

                ServerClient saved = Server.savedClients.Find(fetch => fetch.PlayerData.Username == dummy.PlayerData.Username);
                if (saved != null)
                {
                    saved.PlayerData.Faction = null;
                    StaticProxy.playerHandler.SavePlayerData(saved);
                }
            }

            DisbandFaction(faction);
        }

        public static void PromoteMember(Faction faction, ServerClient memberToPromote)
        {
            ServerClient toPromote = null;
            MemberRank previousRank = 0;

            foreach (KeyValuePair<ServerClient, MemberRank> pair in faction.members)
            {
                if (pair.Key.PlayerData.Username == memberToPromote.PlayerData.Username)
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

        public static void DemoteMember(Faction faction, ServerClient memberToPromote)
        {
            ServerClient toDemote = null;
            MemberRank previousRank = 0;

            foreach (KeyValuePair<ServerClient, MemberRank> pair in faction.members)
            {
                if (pair.Key.PlayerData.Username == memberToPromote.PlayerData.Username)
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

        public static void UpdateAllPlayerDetailsInFaction(Faction faction)
        {
            ServerClient[] dummyfactionMembers = faction.members.Keys.ToArray();

            foreach (ServerClient dummy in dummyfactionMembers)
            {
                ServerClient connected = Networking.connectedClients.Find(fetch => fetch.PlayerData.Username == dummy.PlayerData.Username);
                if (connected != null)
                {
                    Networking.SendData(connected, GetFactionDetails(connected));
                }
            }
        }

        public static MemberRank GetMemberPowers(Faction faction, ServerClient memberToCheck)
        {
            foreach (KeyValuePair<ServerClient, MemberRank> pair in faction.members)
            {
                if (pair.Key.PlayerData.Username == memberToCheck.PlayerData.Username)
                {
                    return pair.Value;
                }
            }

            return MemberRank.Member;
        }
    }
}