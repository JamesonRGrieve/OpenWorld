using System.Collections.Generic;
using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets.Faction
{
    public class FactionManagementPacket : FactionManagementPacketBase
    {
        public override FactionManagementType ManagementType => this.GivenManagementType;

        public FactionManagementType GivenManagementType { get; private set; }

        public FactionManagementPacket(FactionManagementType type)
        {
            this.GivenManagementType = type;
        }

        protected override List<string> GetAdditionalData() => null;
    }
}
