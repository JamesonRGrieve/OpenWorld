using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class VariablesPacket : PacketBase
    {
        public override PacketType Type => PacketType.Variables;

        public bool AllowDevMode { get; }

        public bool Wipe { get; }

        public RoadSystemType RoadSystem { get; }

        public string ServerName { get; }

        public bool UseChat { get; }

        public bool UseChatProfanityFilter { get; }

        public bool ModVerificationForced { get; }

        public bool DifficultyEnforced { get; }

        public VariablesPacket(
            bool allowDevMode,
            bool wipe,
            RoadSystemType roadSystem,
            string serverName,
            bool useChat,
            bool useChatProfanityFilter,
            bool modVerificationForced,
            bool difficultyEnforced)
        {
            this.AllowDevMode = allowDevMode;
            this.Wipe = wipe;
            this.RoadSystem = roadSystem;
            this.ServerName = serverName;
            this.UseChat = useChat;
            this.UseChatProfanityFilter = useChatProfanityFilter;
            this.ModVerificationForced = modVerificationForced;
            this.DifficultyEnforced = difficultyEnforced;
        }

        public override string GetData()
            => this.BuildData(
                "Variables",
                this.BoolToIntString(this.AllowDevMode),
                this.BoolToIntString(this.Wipe),
                ((int)this.RoadSystem).ToString(),
                this.BoolToIntString(this.UseChat),
                this.BoolToIntString(this.UseChatProfanityFilter),
                this.BoolToIntString(this.ModVerificationForced),
                this.BoolToIntString(this.DifficultyEnforced),
                this.ServerName);

        private string BoolToIntString(bool value) => (value ? 1 : 0).ToString();
    }
}
