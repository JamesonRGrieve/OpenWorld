using System;
using System.Collections.Generic;
using System.Text;

namespace OpenWorld.Server
{
    [System.Serializable]
    public class FactionStable : FactionStructure
    {
        public override FactionOld holdingFaction => base.holdingFaction;

        public override string structureName => "Stable";

        public override int structureType => (int)StructureType.Stable;

        public override int structureTile => base.structureTile;

        public FactionStable(FactionOld holdingFaction, int structureTile)
        {
            this.holdingFaction = holdingFaction;
            this.structureTile = structureTile;
        }
    }
}
