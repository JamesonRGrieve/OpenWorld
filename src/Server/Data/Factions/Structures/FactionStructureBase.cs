using OpenWorld.Shared.Enums;

namespace OpenWorld.Server.Data.Factions.Structures
{
    public abstract class FactionStructureBase
    {
        public abstract FactionStructureType Type { get; }

        public int TileId { get; set; }
    }
}
