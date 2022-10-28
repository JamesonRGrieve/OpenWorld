namespace OpenWorldServer.Data.Configs
{
    public class WealthCheckConfig
    {
        public bool IsActive { get; set; } = false;

        public int WarningThreshold { get; set; } = 10000;

        public int BanThreshold { get; set; } = 100000;
    }
}
