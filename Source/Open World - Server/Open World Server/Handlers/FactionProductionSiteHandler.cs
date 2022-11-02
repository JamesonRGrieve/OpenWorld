using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace OpenWorldServer
{
    public static class FactionProductionSiteHandler
    {
        public static void TickProduction()
        {
            while (true)
            {
                Thread.Sleep(600000);

                Faction[] allFactions = Server.savedFactions.ToArray();
                foreach (Faction faction in allFactions)
                {
                    foreach (FactionStructure structure in faction.factionStructures)
                    {
                        if (structure is FactionProductionSite)
                        {
                            SendProductionToMembers(faction);
                            break;
                        }
                    }
                }
            }
        }

        public static void SendProductionToMembers(Faction faction)
        {
            PlayerClient[] dummyfactionMembers = faction.members.Keys.ToArray();
            foreach (PlayerClient dummy in dummyfactionMembers)
            {
                PlayerClient connected = Networking.connectedClients.Find(fetch => fetch.Account.Username == dummy.Account.Username);
                if (connected != null)
                {
                    Networking.SendData(connected, "FactionManagement│ProductionSite│Tick");
                }
            }
        }
    }
}
