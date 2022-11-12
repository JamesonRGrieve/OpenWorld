using System;
using System.Collections.Generic;
using OpenWorld.Shared.Enums;
using OpenWorldServer.Data.Factions.Structures;

namespace OpenWorldServer.Data.Factions
{
    public class Faction
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public int Wealth { get; set; } = 0;

        public Dictionary<Guid, FactionRank> Members = new Dictionary<Guid, FactionRank>();

        public List<FactionStructureBase> Structures { get; set; } = new List<FactionStructureBase>();
    }
}
