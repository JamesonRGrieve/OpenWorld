using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class LoadGamePacket : PacketBase
    {
        public override PacketType Type => PacketType.LoadGame;

        public override string GetData() => "LoadGame" + PacketHandler.PacketDataSplitter;
    }
}
