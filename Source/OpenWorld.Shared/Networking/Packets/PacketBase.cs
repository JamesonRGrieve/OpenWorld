using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public abstract class PacketBase : IPacket
    {
        public abstract PacketType Type { get; }

        public string RawData { get; protected set; }

        public virtual void SetPacket(string data)
        {
            this.RawData = data;
        }

        public abstract string GetData();
    }
}
