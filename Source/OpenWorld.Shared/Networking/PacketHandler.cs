using System;
using OpenWorld.Shared.Networking.Packets;

namespace OpenWorld.Shared.Networking
{
    public static class PacketHandler
    {
        public const char PacketDataSplitter = '│';
        public const char InnerDataSplitter = '»';

        private static IPacket SetupPacket<T>(string data)
            where T : IPacket, new()
        {
            var packet = new T();
            packet.SetPacket(data);

            return packet;
        }

        public static T GetPacket<T>(string data)
            where T : IPacket
            => (T)GetPacket(data);

        public static IPacket GetPacket(string data)
        {
            var splitterIndex = data.IndexOf(PacketDataSplitter);
            splitterIndex = splitterIndex < 0 ? data.Length : splitterIndex;

            var rawType = data.Substring(0, splitterIndex);
            switch (rawType)
            {
                case "Connect":
                    return SetupPacket<ConnectPacket>(data);
                case "SettlementBuilder":
                    return SetupPacket<SettlementBuilderPacket>(data);
                case "ChatMessage":
                    return SetupPacket<ChatMessagePacket>(data);
                case "UserSettlement":
                case "ForceEvent":
                case "SendGiftTo":
                case "SendTradeTo":
                case "SendBarterTo":
                case "TradeStatus":
                case "BarterStatus":
                case "GetSpyInfo":
                case "FactionManagement":
                default:
                    throw new ArgumentException($"No PacketType for '{rawType}' found in {nameof(PacketHandler)}");
            }
        }
    }
}
