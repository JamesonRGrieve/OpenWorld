using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using OpenWorld.Shared.Enums;
using OpenWorld.Server.Converter;

namespace OpenWorld.Server.Data.Factions
{
    public class Faction
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public int Wealth { get; set; } = 0;

        [JsonConverter(typeof(FactionMemberJsonConverter))] // Fix for Json Serializer in Net Core 3.1
        public Dictionary<Guid, FactionRank> Members { get; set; } = new Dictionary<Guid, FactionRank>();

        // We use object here so we dont need an extra Converter. Not a long term solution
        public List<object> Structures { get; set; } = new List<object>();
    }
}
