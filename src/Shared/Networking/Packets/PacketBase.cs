using System.Collections.Generic;
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

        protected string[] SplitData(string data) => data.Split(PacketHandler.PacketDataSplitter);

        // we don't overload this method so we don't have to convert the list into an array first. Its not a huge save 
        protected string BuildData(List<string> splits) => string.Join(PacketHandler.PacketDataSplitter, splits);

        protected string BuildData(params string[] splits) => string.Join(PacketHandler.PacketDataSplitter, splits);
    }
}
