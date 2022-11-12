using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class DisconnectForModsPacket : DisconnectPacket
    {
        public string[] InvalidMods { get; private set; }

        public string[] MissingMods { get; private set; }

        public DisconnectForModsPacket()
        {
        }

        public DisconnectForModsPacket(string[] invalidMods, string[] missingMods)
            : base(DisconnectReason.ModsMismatch)
        {
            this.InvalidMods = invalidMods ?? new string[] { };
            this.MissingMods = missingMods ?? new string[] { };
        }

        public override string GetData()
        {
            var data = base.GetData();
            var mods = string.Empty;

            if (this.InvalidMods.Length != 0)
            {
                mods += string.Join(PacketHandler.InnerDataSplitter, this.InvalidMods);
            }

            if (this.MissingMods.Length != 0)
            {
                if (this.InvalidMods.Length != 0)
                {
                    mods += PacketHandler.InnerDataSplitter;
                }

                mods += string.Join(PacketHandler.InnerDataSplitter, this.MissingMods);
            }
            return this.BuildData(data, mods);
        }
    }
}
