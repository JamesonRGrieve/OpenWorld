namespace OpenWorldServer
{
    public static class JoiningsUtils
    {
        public static string GetGiftsToSend(PlayerClient client)
        {
            string dataToSend = "GiftedItems│";

            if (client.Account.GiftString.Count == 0) return dataToSend;

            else
            {
                string giftsToSend = "";

                foreach (string str in client.Account.GiftString) giftsToSend += str + "»";

                dataToSend += giftsToSend;

                client.Account.GiftString.Clear();

                return dataToSend;
            }
        }

        public static string GetTradesToSend(PlayerClient client)
        {
            string dataToSend = "TradedItems│";

            if (client.Account.TradeString.Count == 0) return dataToSend;

            else
            {
                string tradesToSend = "";

                foreach (string str in client.Account.TradeString) tradesToSend += str + "»";

                dataToSend += tradesToSend;

                client.Account.TradeString.Clear();

                return dataToSend;
            }
        }
    }
}