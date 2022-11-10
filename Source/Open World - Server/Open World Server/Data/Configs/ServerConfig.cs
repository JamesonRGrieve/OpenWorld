using OpenWorld.Shared.Data;
using OpenWorldServer.Data.Configs;

namespace OpenWorldServer.Data
{
    public class ServerConfig
    {
        public string ServerName { get; set; } = "My OpenWorld Server";

        public string Description { get; set; } = string.Empty;

        public string HostIP { get; set; } = "0.0.0.0"; // 0.0.0.0 Listen on all Interfaces

        public int Port { get; set; } = 25555;

        public ushort MaxPlayers { get; set; } = 300;

        public bool AllowDevMode { get; set; } = false;

        public bool ForceDifficulty { get; set; } = false;

        public bool WhitelistMode { get; set; } = false;

        public ModsConfig ModsSystem { get; set; } = new ModsConfig();

        public ChatConfig ChatSystem { get; set; } = new ChatConfig();

        public AntiCheatConfig AntiCheat { get; set; } = new AntiCheatConfig();

        public IdleConfig IdleSystem { get; set; } = new IdleConfig();

        public RoadConfig RoadSystem { get; set; } = new RoadConfig();

        public PlanetConfig Planet { get; set; } = new PlanetConfig();
    }
}
