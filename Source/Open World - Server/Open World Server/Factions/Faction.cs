using System;
using System.Collections.Generic;
using System.Text;
using OpenWorldServer.Handlers.Old;

namespace OpenWorldServer
{
    [System.Serializable]
    public class Faction
    {
        public string name = "";
        public int wealth = 0;
        public Dictionary<PlayerClient, FactionHandler.MemberRank> members = new Dictionary<PlayerClient, FactionHandler.MemberRank>();
        public List<FactionStructure> factionStructures = new List<FactionStructure>();
    }
}