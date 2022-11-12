using OpenWorld.Shared.Data;
using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class PlanetPacket : PacketBase
    {
        public override PacketType Type => PacketType.PlanetData;

        public PlanetConfig PlanetData { get; private set; }

        public PlanetPacket()
        {
        }

        public PlanetPacket(PlanetConfig planetData)
        {
            this.PlanetData = planetData;
        }

        public override string GetData() => this.BuildData(
                "Planet",
                this.PlanetData.GlobeCoverage.ToString(),
                this.PlanetData.Seed,
                this.PlanetData.OverallRainfall.ToString(),
                this.PlanetData.OverallTemperature.ToString(),
                this.PlanetData.OverallPopulation.ToString());
    }
}
