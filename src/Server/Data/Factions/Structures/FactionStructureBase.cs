using OpenWorld.Shared.Enums;

namespace OpenWorldServer.Data.Factions.Structures
{
    public abstract class FactionStructureBase
    {
        public abstract FactionStructureType Type { get; }

        public int TileId { get; set; }
    }
}
