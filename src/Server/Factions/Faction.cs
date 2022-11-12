using System.Collections.Generic;
using OpenWorldServer.Handlers.Old;

namespace OpenWorldServer
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