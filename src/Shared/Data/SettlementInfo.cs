using System;

namespace OpenWorld.Shared.Data
{
    public class SettlementInfo
    {
        public Guid OwnerId { get; }

        public string Owner { get; }

        public string HomeTileId { get; }

        public string FactionName { get; }

        public SettlementInfo(Guid ownerId, string owner, string homeTileId, string factionName)
        {
            this.OwnerId = ownerId;
            this.Owner = owner;
            this.HomeTileId = homeTileId;
            this.FactionName = factionName;
        }
    }
}
