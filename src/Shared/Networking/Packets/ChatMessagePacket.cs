using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class ChatMessagePacket : PacketBase
    {
        public override PacketType Type => PacketType.ChatMessage;

        public string Sender { get; private set; }

        public string Message { get; private set; }

        public ChatMessagePacket()
        {
        }

        public ChatMessagePacket(string sender, string message)
        {
            this.Sender = sender;
            this.Message = message;
        }

        public override void SetPacket(string data)
        {
            base.SetPacket(data);
            var splits = this.SplitData(data);

            this.Sender = splits[1];
            this.Message = splits[2];
        }

        public override string GetData() => this.BuildData("ChatMessage", this.Sender, this.Message);
    }
}
