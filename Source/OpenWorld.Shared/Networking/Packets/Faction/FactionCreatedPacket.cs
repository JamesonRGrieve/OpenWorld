using System.Collections.Generic;
using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets.Faction
{
    public class FactionCreatedPacket : FactionManagementPacketBase
    {
        public override FactionManagementType ManagementType => FactionManagementType.Created;

        protected override List<string> GetAdditionalData() => null;
    }
}
