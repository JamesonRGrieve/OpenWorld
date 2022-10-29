using System;

namespace OpenWorldServer.Data
{
    public class BanInfo
    {
        public string Username { get; set; } = string.Empty;

        public string IPAddress { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public DateTime? BanDate { get; set; } = null;

        //public DateTime? BanEnds { get; set; } = null;
    }
}
