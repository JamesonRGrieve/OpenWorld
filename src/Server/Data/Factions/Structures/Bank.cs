using OpenWorld.Shared.Enums;

namespace OpenWorld.Server.Data.Factions.Structures
{
    internal class Bank : FactionStructureBase
    {
        public override FactionStructureType Type => FactionStructureType.Bank;

        public int DepositedSilver { get; set; }
    }
}
