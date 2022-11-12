namespace OpenWorld.Server.Data.Configs
{
    public class WealthCheckConfig
    {
        public bool IsActive { get; set; } = false;

        public int WarningThreshold { get; set; } = 50000;

        public int BanThreshold { get; set; } = 250000;
    }
}
