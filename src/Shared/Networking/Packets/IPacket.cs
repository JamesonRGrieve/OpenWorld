using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public interface IPacket
    {
        PacketType Type { get; }

        string RawData { get; }

        string GetData();

        void SetPacket(string data);
    }
}
