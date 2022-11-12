using System.Collections.Generic;
using System.Linq;
using OpenWorld.Shared.Data;
using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class SettlementsPacket : PacketBase
    {
        private const string SettlementInfoSplitter = ":";

        public override PacketType Type => PacketType.Settlements;

        public IEnumerable<SettlementInfo> Settlements { get; private set; }

        public string OwnFactionName { get; private set; }

        public SettlementsPacket()
        {
        }

        public SettlementsPacket(IEnumerable<SettlementInfo> settlements, string ownFactionName)
        {
            this.Settlements = settlements;
            this.OwnFactionName = ownFactionName;
        }

        public override string GetData()
        {
            var data = "Settlements" + PacketHandler.PacketDataSplitter;
            if (this.Settlements.Count() == 0)
            {
                return data;
            }

            var settlementDataList = new List<string>();
            foreach (var settlement in this.Settlements)
            {
                var factionType = SettlementFactionType.NoFaction;
                if (!string.IsNullOrEmpty(this.OwnFactionName) && settlement.FactionName == this.OwnFactionName)
                {
                    factionType = SettlementFactionType.SameFaction;
                }
                else if (!string.IsNullOrEmpty(settlement.FactionName))
                {
                    factionType = SettlementFactionType.OtherFaciton;
                }

                var settlementData = string.Join(SettlementInfoSplitter, settlement.HomeTileId, settlement.Owner, (int)factionType);
                settlementDataList.Add(settlementData);
            }

            return data + this.BuildData(settlementDataList) + PacketHandler.PacketDataSplitter;
        }
    }
}
