using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class PingPacket : PacketBase
    {
        public override PacketType Type => PacketType.Ping;

        public override string GetData() => "Ping";
    }
}
