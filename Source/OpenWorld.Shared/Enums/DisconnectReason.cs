namespace OpenWorld.Shared.Enums
{
    public enum DisconnectReason
    {
        Unkown = 0,
        Corrupted = 1,
        ServerFull = 2,

        WrongPassword = 10,
        AlreadyLoggedIn = 11,
        NotOnWhitelist = 12,

        VersionMismatch = 20,
        ModsMismatch = 21,

        Kicked = 30,
        Banned = 31,
    }
}
