using System;
using System.Collections.Generic;
using System.Text;

namespace OpenWorld.Server.Deprecated
{
    [System.Serializable]
    public class ServerClient
    {

        public string username = "";
        public string password = "";
        public bool isAdmin = false;
        public bool toWipe = false;

        //Relevant Data
        public string homeTileID;
        public List<string> giftString = new List<string>();
        public List<string> tradeString = new List<string>();
        public FactionOld faction;

        //Wealth Data
        public int pawnCount;
        public float wealth;

        //Variables Data
        public bool isImmunized = false;
    }
}
