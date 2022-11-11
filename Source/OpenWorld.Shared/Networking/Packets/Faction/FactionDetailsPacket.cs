using System.Collections.Generic;
using System.Linq;
using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets.Faction
{
    public class FactionDetailsPacket : FactionManagementPacketBase
    {
        private const char memberInfoSplitter = ':';

        public override FactionManagementType ManagementType => FactionManagementType.Details;

        public IEnumerable<(string Username, FactionRank Rank)> Members { get; private set; }

        public FactionDetailsPacket(IEnumerable<(string Username, FactionRank Rank)> members)
        {
            this.Members = members;
        }

        public override string GetData()
        {
            var data = base.GetData();
            if (this.Members == null || !this.Members.Any())
            {
                return data + PacketHandler.PacketDataSplitter;
            }

            return data;
        }

        protected override List<string> GetAdditionalData()
        {
            if (this.Members == null || !this.Members.Any())
            {
                return null;
            }

            var memberInfoSplit = this.Members.Where(m => !string.IsNullOrEmpty(m.Username)).Select(m => $"{m.Username}{memberInfoSplitter}{(int)m.Rank}");
            var membersSplit = string.Join(PacketHandler.InnerDataSplitter, memberInfoSplit);
            return new List<string>() { membersSplit };
        }
    }
}
