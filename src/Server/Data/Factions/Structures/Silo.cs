using System.Collections.Generic;
using OpenWorld.Shared.Enums;

namespace OpenWorld.Server.Data.Factions.Structures
{
    internal class Silo : FactionStructureBase
    {
        public override FactionStructureType Type => FactionStructureType.Silo;

        public Dictionary<int, List<string>> Items { get; set; } = new Dictionary<int, List<string>>();
    }
}
