using System.Linq;
using System.Threading;

namespace OpenWorldServer.Handlers.Old
{
    public static class FactionBankHandler
    {
        public static void TickBank()
        {
            while (true)
            {
                Thread.Sleep(600000);

                Faction[] allFactions = Server.savedFactions.ToArray();
                foreach (Faction faction in allFactions)
                {
                    FactionBank bankToFind = faction.factionStructures.Find(fetch => fetch is FactionBank) as FactionBank;

                    if (bankToFind == null) continue;
                    else
                    {
                        bankToFind.depositedSilver += 100;
                        FactionHandler.SaveFaction(faction);
                    }
                }

                ConsoleUtils.LogToConsole("[Factions Bank Tick]");
            }
        }

        public static void DepositMoney(Faction faction, int quantity)
        {
            FactionBank bankToFind = faction.factionStructures.Find(fetch => fetch is FactionBank) as FactionBank;

            if (bankToFind == null) return;

            bankToFind.depositedSilver += quantity;

            FactionHandler.SaveFaction(faction);

            RefreshMembersBankDetails(faction);
        }

        public static void WithdrawMoney(Faction faction, int quantity, PlayerClient client)
        {
            FactionBank bankToFind = faction.factionStructures.Find(fetch => fetch is FactionBank) as FactionBank;

            if (bankToFind == null) return;

            if (bankToFind.depositedSilver - quantity < 0) return;
            else bankToFind.depositedSilver -= quantity;

            Networking.SendData(client, "FactionManagement│Bank│Withdraw" + "│" + quantity);

            FactionHandler.SaveFaction(faction);

            RefreshMembersBankDetails(faction);
        }

        public static void RefreshMembersBankDetails(Faction faction)
        {
            FactionBank bankToFind = faction.factionStructures.Find(fetch => fetch is FactionBank) as FactionBank;

            if (bankToFind == null) return;

            var dummyFactionMembers = faction.members.Keys.ToArray();
            foreach (var dummy in dummyFactionMembers)
            {
                var connected = StaticProxy.playerManager.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == dummy.Account.Username);
                if (connected != null)
                {
                    Networking.SendData(connected, "FactionManagement│Bank│Refresh│" + bankToFind.depositedSilver);
                }
            }
        }
    }
}
