using System;

namespace OpenWorldServer.Data.Configs
{
    public class WorldConfig
    {
        public double GlobeCoverage { get; set; } = 0.3;

        public string Seed { get; set; } = "Random_Seed_" + Guid.NewGuid().ToString();

        public byte OverallRainfall { get; set; } = 3;

        public byte OverallTemperature { get; set; } = 3;

        public byte OverallPopulation { get; set; } = 3;
    }
}
