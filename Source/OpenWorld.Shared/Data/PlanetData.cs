using System;

namespace OpenWorld.Shared.Data
{
    public class PlanetData
    {
        public double GlobeCoverage { get; set; } = 0.3;

        public string Seed { get; set; } = "Random_Seed_" + Guid.NewGuid().ToString();

        public byte OverallRainfall { get; set; } = 3;

        public byte OverallTemperature { get; set; } = 3;

        public byte OverallPopulation { get; set; } = 3;
    }
}
