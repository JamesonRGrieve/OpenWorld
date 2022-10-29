namespace OpenWorldServer.Data
{
    public class BannedInfo
    {
        public string Username { get; set; } = string.Empty;

        public string IPAddress { get; set; } = string.Empty;

        // Extended Ban Functionallity which could be implemented later
        //public string Reason { get; set; } = string.Empty;

        //public DateTime? BannedStart { get; set; } = null;

        //public DateTime? BannedEnd { get; set; } = null;
    }
}
