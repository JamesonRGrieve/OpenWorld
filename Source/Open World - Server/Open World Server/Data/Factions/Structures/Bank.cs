using OpenWorld.Shared.Enums;

namespace OpenWorldServer.Data.Factions.Structures
{
    internal class Bank : FactionStructureBase
    {
        public override FactionStructureType Type => FactionStructureType.Bank;

        public int DepositedSilver { get; set; }
    }
}
