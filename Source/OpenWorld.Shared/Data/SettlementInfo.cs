namespace OpenWorld.Shared.Data
{
    public class SettlementInfo
    {
        public string HomeTileId { get; }

        public string Owner { get; }

        public string FactionName { get; }

        public SettlementInfo(string homeTileId, string owner, string factionName)
        {
            this.HomeTileId = homeTileId;
            this.Owner = owner;
            this.FactionName = factionName;
        }
    }
}
