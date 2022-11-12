using System;
using System.Collections.Generic;
using System.Text;

namespace OpenWorld.Server
{
    [System.Serializable]
    public class FactionWonder : FactionStructure
    {
        public override FactionOld holdingFaction => base.holdingFaction;

        public override string structureName => "Wonder Structure";

        public override int structureType => (int)StructureType.Wonder;

        public override int structureTile => base.structureTile;

        public FactionWonder(FactionOld holdingFaction, int structureTile)
        {
            this.holdingFaction = holdingFaction;
            this.structureTile = structureTile;
        }
    }
}
