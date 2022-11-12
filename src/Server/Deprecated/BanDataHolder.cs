using System.Collections.Generic;

namespace OpenWorld.Server.Deprecated
{
    [System.Serializable]
    public class BanDataHolder
    {
        public Dictionary<string, string> BannedIPs { get; set; } = new Dictionary<string, string>();

        public BanDataHolder(Dictionary<string, string> bannedIPs)
        {
            BannedIPs = bannedIPs;
        }
    }
}
