using System;
using System.Collections.Generic;
using System.Text;

namespace OpenWorld.Server
{
    [System.Serializable]
    public class FactionMarketplace : FactionStructure
    {
        public override FactionOld holdingFaction => base.holdingFaction;

        public override string structureName => "Marketplace";

        public override int structureType => (int)StructureType.Marketplace;

        public override int structureTile => base.structureTile;

        public FactionMarketplace(FactionOld holdingFaction, int structureTile)
        {
            this.holdingFaction = holdingFaction;
            this.structureTile = structureTile;
        }
    }
}
