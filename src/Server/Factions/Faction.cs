using System.Collections.Generic;
using OpenWorld.Server.Handlers.Old;

namespace OpenWorld.Server
{
    [System.Serializable]
    public class FactionOld
    {
        public string name = "";
        public int wealth = 0;
        public Dictionary<PlayerClient, FactionHandler.MemberRank> members = new Dictionary<PlayerClient, FactionHandler.MemberRank>();
        public List<FactionStructure> factionStructures = new List<FactionStructure>();
    }
}