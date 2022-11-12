using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class PlayerListPacket : PacketBase
    {
        const char usernameSeperator = ':';

        public override PacketType Type => PacketType.PlayerList;

        public string[] Usernames { get; private set; }

        public int PlayerCount { get; private set; }

        public PlayerListPacket()
        {
        }

        public PlayerListPacket(string[] usernames, int playerCount)
        {
            this.Usernames = usernames;
            this.PlayerCount = playerCount;
        }

        public override void SetPacket(string data)
        {
            base.SetPacket(data);
            var splits = this.SplitData(data);

            if (splits.Length > 2)
            {
                this.Usernames = splits[1].Split(usernameSeperator);
            }
        }

        public override string GetData()
        {
            var result = "PlayerList" + PacketHandler.PacketDataSplitter;
            if ((this.Usernames?.Length ?? 0) > 0)
            {
                var names = string.Join(usernameSeperator, this.Usernames);
                result += this.BuildData(names, this.PlayerCount.ToString());
            }
            else
            {
                result += this.BuildData(string.Empty, this.PlayerCount.ToString());
            }

            return result;
        }
    }
}
