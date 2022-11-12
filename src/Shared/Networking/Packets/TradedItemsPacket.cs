using System.Collections.Generic;
using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class TradedItemsPacket : PacketBase
    {
        public override PacketType Type => PacketType.TradedItems;

        public IEnumerable<string> TradesToSend { get; private set; }

        public TradedItemsPacket()
        {
        }

        public TradedItemsPacket(IEnumerable<string> tradesToSend)
        {
            this.TradesToSend = tradesToSend;
        }

        public override void SetPacket(string data)
        {
            base.SetPacket(data);
            var splits = this.SplitData(data);
            var trades = new List<string>();
            foreach (var item in splits[1..]) // skip the first and from there
            {
                trades.Add(item);
            }

            this.TradesToSend = trades;
        }

        public override string GetData() => this.BuildData("TradedItems", string.Join(PacketHandler.InnerDataSplitter, this.TradesToSend));
    }
}
