namespace OpenWorld.Shared.Enums
{
    public enum PacketType : byte
    {
        Unkown = 0,
        Ping = 1,

        Connect = 10,
        Disconnect = 11,
        PlayerList = 12,

        SettlementBuilder = 20,
    }
}
