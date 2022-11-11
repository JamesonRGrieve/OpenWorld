using System.Linq;
using System.Threading;

namespace OpenWorldServer.Handlers.Old
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
                    FactionStructure productionSiteToFind = faction.factionStructures.Find(fetch => fetch is FactionProductionSite);

                    if (productionSiteToFind == null) continue;
                    else SendProductionToMembers(faction);
                }

                ConsoleUtils.LogToConsole("[Factions Production Site Tick]");
            }
        }

        public static void SendProductionToMembers(Faction faction)
        {
            var dummyfactionMembers = faction.members.Keys.ToArray();
            int productionSitesInFaction = faction.factionStructures.FindAll(fetch => fetch is FactionProductionSite).Count();
            foreach (var dummy in dummyfactionMembers)
            {
                PlayerClient connected = StaticProxy.playerManager.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == dummy.Account.Username);
                if (connected != null)
                {
                    Networking.SendData(connected, "FactionManagement│ProductionSite│Tick" + "│" + productionSitesInFaction);
                }
            }
        }
    }
}
