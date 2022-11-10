namespace OpenWorld.Shared.Enums
{
    public enum PacketType : byte
    {
        Unkown = 0,
        Ping = 1,
        ChatMessage = 2,

        Connect = 10,
        Disconnect = 11,
        NewGame = 12,
        LoadGame = 13,

        PlayerList = 20,
        PlanetData = 21,
        Variables = 22,

        Settlements = 30,
        SettlementBuilder = 31,


    }
}
