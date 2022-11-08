using System;
using System.Linq;

namespace OpenWorldServer
{
    public static class AdvancedCommands
    {
        public static void SayCommand(string[] arguments)
        {
            ConsoleUtils.LogToConsole($"Chat - [Console] {arguments[0]}");
            Server.chatCache.Add($"[{DateTime.Now}] │ {arguments[0]}");
            foreach (PlayerClient sc in StaticProxy.playerHandler.ConnectedClients.ToArray()) Networking.SendData(sc, $"ChatMessage│SERVER│{arguments[0]}");
        }
        public static void BroadcastCommand(string[] arguments)
        {
            foreach (PlayerClient sc in StaticProxy.playerHandler.ConnectedClients.ToArray()) Networking.SendData(sc, $"Notification│{arguments[0]}");
            ConsoleUtils.LogToConsole("Letter Sent To Every Connected Player", ConsoleUtils.ConsoleLogMode.Info);
        }
        public static void NotifyCommand(string[] arguments)
        {
            PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == arguments[0]);
            Networking.SendData(targetClient, $"Notification│{arguments[1]}");
            ConsoleUtils.LogToConsole($"Sent Letter To {targetClient.Account.Username}", ConsoleUtils.ConsoleLogMode.Info);
        }
        public static void GiveItemCommand(string[] arguments)
        {
            PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == arguments[0]);
            Networking.SendData(targetClient, $"GiftedItems│{arguments[1]}┼{arguments[2]}┼{arguments[3]}┼");
            ConsoleUtils.LogToConsole($"Item Has Neen Gifted To Player {targetClient.Account.Username}", ConsoleUtils.ConsoleLogMode.Info);
        }
        public static void GiveItemAllCommand(string[] arguments)
        {
            foreach (PlayerClient client in StaticProxy.playerHandler.ConnectedClients.ToArray()) Networking.SendData(client, $"GiftedItems│{arguments[0]}┼{arguments[1]}┼{arguments[2]}┼");
            ConsoleUtils.LogToConsole("Item Has Neen Gifted To All Players", ConsoleUtils.ConsoleLogMode.Info);
        }
        public static void ImmunizeCommand(string[] arguments)
        {
            PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == arguments[0]);
            targetClient.Account.IsImmunized = true;
            Server.savedClients.Find(fetch => fetch.Account.Username == targetClient.Account.Username).Account.IsImmunized = true;
            StaticProxy.playerHandler.AccountsHandler.SaveAccount(targetClient);
            ConsoleUtils.LogToConsole($"Player {targetClient.Account.Username} Has Been Immunized", ConsoleUtils.ConsoleLogMode.Info);
        }
        public static void DeimmunizeCommand(string[] arguments)
        {
            PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == arguments[0]);
            targetClient.Account.IsImmunized = false;
            Server.savedClients.Find(fetch => fetch.Account.Username == targetClient.Account.Username).Account.IsImmunized = false;
            StaticProxy.playerHandler.AccountsHandler.SaveAccount(targetClient);
            ConsoleUtils.LogToConsole($"Player {targetClient.Account.Username} Has Been Deimmunized", ConsoleUtils.ConsoleLogMode.Info);
        }
        public static void ProtectCommand(string[] arguments)
        {
            PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == arguments[0]);
            targetClient.IsEventProtected = true;
            Server.savedClients.Find(fetch => fetch.Account.Username == targetClient.Account.Username).IsEventProtected = true;
            StaticProxy.playerHandler.AccountsHandler.SaveAccount(targetClient);
            ConsoleUtils.LogToConsole($"Player {targetClient.Account.Username} Has Been Protected", ConsoleUtils.ConsoleLogMode.Info);
        }
        public static void DeprotectCommand(string[] arguments)
        {
            PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == arguments[0]);
            targetClient.IsEventProtected = false;
            Server.savedClients.Find(fetch => fetch.Account.Username == targetClient.Account.Username).IsEventProtected = false;
            StaticProxy.playerHandler.AccountsHandler.SaveAccount(targetClient);
            ConsoleUtils.LogToConsole($"Player {targetClient.Account.Username} Has Been Protected", ConsoleUtils.ConsoleLogMode.Info);
        }
        public static void InvokeCommand(string[] arguments)
        {
            PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == arguments[0]);
            Networking.SendData(targetClient, "ForcedEvent│" + arguments[1]);
            ConsoleUtils.LogToConsole($"Sent Event {arguments[1]} to {targetClient.Account.Username}", ConsoleUtils.ConsoleLogMode.Info);
        }
        public static void PlagueCommand(string[] arguments)
        {
            foreach (PlayerClient client in StaticProxy.playerHandler.ConnectedClients.ToArray()) Networking.SendData(client, "ForcedEvent│" + arguments[0]);
            ConsoleUtils.LogToConsole($"Sent Event {arguments[0]} To Every Player", ConsoleUtils.ConsoleLogMode.Info);
        }
        public static void PromoteCommand(string[] arguments)
        {
            PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == arguments[0]);
            if (targetClient.Account.IsAdmin == true) ConsoleUtils.LogToConsole($"Player {targetClient.Account.Username} Was Already An Administrator", ConsoleUtils.ConsoleLogMode.Info);
            else
            {
                targetClient.Account.IsAdmin = true;
                Server.savedClients.Find(fetch => fetch.Account.Username == arguments[0]).Account.IsAdmin = true;
                StaticProxy.playerHandler.AccountsHandler.SaveAccount(targetClient);
                Networking.SendData(targetClient, "Admin│Promote");
                ConsoleUtils.LogToConsole($"Player {targetClient.Account.Username} Has Been Promoted", ConsoleUtils.ConsoleLogMode.Info);
            }
        }
        public static void DemoteCommand(string[] arguments)
        {
            PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == arguments[0]);
            if (!targetClient.Account.IsAdmin) ConsoleUtils.LogToConsole($"Player {targetClient.Account.Username} Is Not An Administrator", ConsoleUtils.ConsoleLogMode.Info);
            else
            {
                targetClient.Account.IsAdmin = false;
                Server.savedClients.Find(fetch => fetch.Account.Username == targetClient.Account.Username).Account.IsAdmin = false;
                StaticProxy.playerHandler.AccountsHandler.SaveAccount(targetClient);
                Networking.SendData(targetClient, "Admin│Demote");
                ConsoleUtils.LogToConsole($"Player {targetClient.Account.Username} Has Been Demoted", ConsoleUtils.ConsoleLogMode.Info);
            }
        }
        public static void PlayerDetailsCommand(string[] arguments)
        {
            PlayerClient liveClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == arguments[0]), savedClient = Server.savedClients.Find(fetch => fetch.Account.Username == arguments[0]);
            bool isConnected = liveClient != null;
            string ip = liveClient == null ? "N/A - Offline" : liveClient.IPAddress.ToString();
            ConsoleUtils.LogToConsole("Player Details", ConsoleUtils.ConsoleLogMode.Heading);
            ConsoleUtils.LogToConsole($"Username: {savedClient.Account.Username}\nPassword: {savedClient.Account.Password}\n");
            ConsoleUtils.LogToConsole("Role", ConsoleUtils.ConsoleLogMode.Heading);
            ConsoleUtils.LogToConsole($"Admin: {savedClient.Account.IsAdmin}");
            ConsoleUtils.LogToConsole("Status", ConsoleUtils.ConsoleLogMode.Heading);
            ConsoleUtils.LogToConsole($"Online: {isConnected}\nConnection IP: {ip}\nImmunized: {savedClient.Account.IsImmunized}\nEvent Shielded: {savedClient.IsEventProtected}\nIn RTSE: {savedClient.InRTSE}");
            ConsoleUtils.LogToConsole("Wealth", ConsoleUtils.ConsoleLogMode.Heading);
            ConsoleUtils.LogToConsole($"Stored Gifts: {savedClient.Account.GiftString.Count}\nStored Trades: {savedClient.Account.TradeString.Count}\nWealth Value: {savedClient.Account.Wealth}\nPawn Count: {savedClient.Account.PawnCount}");
            ConsoleUtils.LogToConsole("Details", ConsoleUtils.ConsoleLogMode.Heading);
            ConsoleUtils.LogToConsole($"Home Tile ID: {savedClient.Account.HomeTileId}\nFaction: {(savedClient.Account.Faction == null ? "None" : savedClient.Account.Faction.name)}");
        }
        public static void FactionDetailsCommand(string[] arguments)
        {
            Faction factionToSearch = Server.savedFactions.Find(fetch => fetch.name == arguments[0]);
            if (factionToSearch == null) ConsoleUtils.LogToConsole($"Faction {arguments[0]} Was Not Found", ConsoleUtils.ConsoleLogMode.Info);
            else
            {
                ConsoleUtils.LogToConsole($"Faction Details Of {factionToSearch.name}", ConsoleUtils.ConsoleLogMode.Heading);
                ConsoleUtils.LogToConsole("Members", ConsoleUtils.ConsoleLogMode.Heading);
                ConsoleUtils.LogToConsole(string.Join('\n', factionToSearch.members.Select(x => $"[{x.Value}] - {x.Key.Account.Username}")));
                ConsoleUtils.LogToConsole("Wealth", ConsoleUtils.ConsoleLogMode.Heading);
                ConsoleUtils.LogToConsole(factionToSearch.wealth.ToString());
                ConsoleUtils.LogToConsole("Structures", ConsoleUtils.ConsoleLogMode.Heading);
                ConsoleUtils.LogToConsole(factionToSearch.factionStructures.Count == 0 ? "No Structures" : string.Join('\n', factionToSearch.factionStructures.Select(x => $"[{x.structureTile}] - {x.structureName}")));
            }
        }
        public static void BanCommand(string[] arguments)
        {
            PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == arguments[0]);
            StaticProxy.playerHandler.BanlistHandler.BanPlayer(targetClient);

        }
        public static void PardonCommand(string[] arguments)
        {
            try
            {
                StaticProxy.playerHandler.BanlistHandler.UnbanPlayer(arguments[0]);
            }
            catch
            {
                ConsoleUtils.LogToConsole($"Player {arguments[0]} Not Found", ConsoleUtils.ConsoleLogMode.Info);
            }
        }
        public static void KickCommand(string[] arguments)
        {
            PlayerClient targetClient = StaticProxy.playerHandler.ConnectedClients.FirstOrDefault(fetch => fetch.Account.Username == arguments[0]);
            targetClient.IsDisconnecting = true;
            ConsoleUtils.LogToConsole("Player {targetClient.Account.Username} Has Been Kicked", ConsoleUtils.ConsoleLogMode.Info);
        }
    }
}