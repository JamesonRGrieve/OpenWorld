using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class NewGamePacket : PacketBase
    {
        public override PacketType Type => PacketType.NewGame;

        public override string GetData() => "NewGame" + PacketHandler.PacketDataSplitter;
    }
}
