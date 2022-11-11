using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace OpenWorldServer.Data
{
    [DebuggerDisplay("{Username,nq} ({Id,nq})")]
    public class PlayerData
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Username { get; set; }

        public string Password { get; set; }

        public bool IsAdmin { get; set; } = false;

        public bool ToWipe { get; set; } = false;

        public DateTime? LastLogin { get; set; }

        [JsonIgnore]
        public bool HasSettlement => !string.IsNullOrEmpty(this.HomeTileId);

        public string HomeTileId { get; set; }

        public List<string> GiftString { get; set; } = new List<string>();

        public List<string> TradeString { get; set; } = new List<string>();

        public Faction Faction { get; set; }

        public int PawnCount { get; set; } = 0;

        public float Wealth { get; set; } = 0f;

        public bool IsImmunized { get; set; } = false;
    }
}
