using System;
using OpenWorld.Shared.Enums;
using OpenWorldServer.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class ConnectPacket : PacketBase
    {
        public override PacketType Type => PacketType.Connect;

        public string Username { get; private set; }

        public string Password { get; private set; }

        public string Version { get; private set; }

        public JoinMode JoinMode { get; private set; }

        public string[] Mods { get; private set; }

        public override string GetData()
        {
            // Needs implementation when refactoring client stuff
            throw new NotImplementedException();
        }

        public override void SetPacket(string data)
        {
            base.SetPacket(data);
            var splits = this.SplitData(data);

            this.Username = splits[1].ToLower();
            this.Password = splits[2];
            this.Version = splits[3];
            this.JoinMode = this.ParseJoinMode(splits[4]);

            if (splits.Length >= 5)
            {
                this.Mods = splits[5].Split(PacketHandler.InnerDataSplitter);
            }
        }

        private JoinMode ParseJoinMode(string joinMode)
        {
            // We cant use this since a typo could be a bigger problem.
            // When the protocol is changed to send a byte for the JoinMode, we can use it to parse by casting.

            switch (joinMode)
            {
                case "NewGame":
                    return JoinMode.NewGame;
                case "LoadGame":
                    return JoinMode.LoadGame;
                default:
                    throw new ArgumentException($"JoinMode '{joinMode}' is not a vaild JoinMode");
            }
        }
    }
}
