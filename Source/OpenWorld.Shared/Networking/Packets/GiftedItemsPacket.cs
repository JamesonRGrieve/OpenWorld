using System.Collections.Generic;
using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class GiftedItemsPacket : PacketBase
    {
        public override PacketType Type => PacketType.GiftedItems;

        public IEnumerable<string> GiftsToSend { get; private set; }

        public GiftedItemsPacket()
        {
        }

        public GiftedItemsPacket(IEnumerable<string> giftsToSend)
        {
            this.GiftsToSend = giftsToSend;
        }

        public override void SetPacket(string data)
        {
            base.SetPacket(data);
            var splits = this.SplitData(data);
            var gifts = new List<string>();
            foreach (var item in splits[1..]) // skip the first and from there
            {
                gifts.Add(item);
            }

            this.GiftsToSend = gifts;
        }

        public override string GetData() => this.BuildData("GiftedItems", string.Join(PacketHandler.InnerDataSplitter, this.GiftsToSend));
    }
}
