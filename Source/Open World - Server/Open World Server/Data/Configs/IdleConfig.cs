namespace OpenWorldServer.Data.Configs
{
    public class IdleConfig
    {
        public bool IsActive { get; set; } = true;

        public uint IdleThresholdInDays { get; set; } = 14;
    }
}
