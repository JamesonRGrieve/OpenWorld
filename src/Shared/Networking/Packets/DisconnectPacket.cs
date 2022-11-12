using System;
using System.Collections.Generic;
using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class DisconnectPacket : PacketBase
    {
        public override PacketType Type => PacketType.Disconnect;

        public DisconnectReason Reason { get; private set; }

        public DisconnectPacket()
        {
        }

        public DisconnectPacket(DisconnectReason reason)
        {
            this.Reason = reason;
        }

        public override string GetData()
        {
            var splits = new List<string>() { "Disconnect", this.ParseReason(this.Reason) };
            return this.BuildData(splits);
        }

        public override void SetPacket(string data)
        {
            base.SetPacket(data);
            // Needs implementation when refactoring client stuff?
        }

        private string ParseReason(DisconnectReason reason)
        {
            switch (reason)
            {
                case DisconnectReason.Corrupted:
                    return "Corrupted";
                case DisconnectReason.ServerFull:
                    return "ServerFull";
                case DisconnectReason.WrongPassword:
                    return "WrongPassword";
                case DisconnectReason.AlreadyLoggedIn:
                    return "AnotherLogin";
                case DisconnectReason.NotOnWhitelist:
                    return "Whitelist";
                case DisconnectReason.VersionMismatch:
                    return "Version";
                case DisconnectReason.ModsMismatch:
                    return "WrongMods";
                case DisconnectReason.Banned:
                    return "Banned";
                case DisconnectReason.Kicked:
                case DisconnectReason.Unkown:
                default:
                    throw new ArgumentException($"{reason} is currently not supported");
            }
        }
    }
}
