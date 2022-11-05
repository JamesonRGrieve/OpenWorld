using System;
using System.Collections.Generic;
using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets
{
    public class SettlementBuilderPacket : PacketBase
    {
        public override PacketType Type => PacketType.SettlementBuilder;

        public SettlementBuilderAction Action { get; private set; } = SettlementBuilderAction.NotSet;

        public string TileId { get; private set; }

        public string Username { get; private set; }

        public int FactionValue { get; private set; }

        public SettlementBuilderPacket()
        {
        }

        /// <summary>
        /// Creates as a Remove Action
        /// </summary>
        /// <param name="tileId"></param>
        public SettlementBuilderPacket(string tileId)
        {
            this.Action = SettlementBuilderAction.Remove;
            this.TileId = tileId;
        }

        /// <summary>
        /// Creates as a Add Action
        /// </summary>
        /// <param name="tileId"></param>
        /// <param name="username"></param>
        /// <param name="factionValue"></param>
        public SettlementBuilderPacket(string tileId, string username, int factionValue)
        {
            this.Action = SettlementBuilderAction.Add;
            this.TileId = tileId;
            this.Username = username;
            this.FactionValue = factionValue;
        }

        public override string GetData()
        {
            var splits = new List<string>() { "SettlementBuilder", this.GetSettlementBuilderAction(this.Action), this.TileId };
            if (this.Action == SettlementBuilderAction.Add)
            {
                splits.Add(this.Username);
                splits.Add(this.FactionValue.ToString());
            }

            return this.BuildData(splits);
        }

        public override void SetPacket(string data)
        {
            base.SetPacket(data);
            var splits = this.SplitData(data);

            if (splits.Length < 2 ||
                string.IsNullOrEmpty(splits[1]) ||
                string.IsNullOrEmpty(splits[2]))
            {
                throw new ArgumentException($"Invalid {nameof(SettlementBuilderPacket)}. Missing Data to determine Action");
            }

            this.Action = this.GetSettlementBuilderAction(splits[1]);
            this.TileId = splits[2];

            if (this.Action == SettlementBuilderAction.Add)
            {

                if (splits.Length < 5 ||
                    string.IsNullOrEmpty(splits[4]))
                {
                    throw new ArgumentException($"Invalid {nameof(SettlementBuilderPacket)}. Action was {this.Action} but is missing corresponding data");
                }
            }

            this.Username = splits[3];
            this.FactionValue = int.Parse(splits[4]);
        }

        private SettlementBuilderAction GetSettlementBuilderAction(string action)
        {
            switch (action)
            {
                case "AddSettlement":
                    return SettlementBuilderAction.Add;
                case "RemoveSettlement":
                    return SettlementBuilderAction.Remove;
            }

            throw new ArgumentException($"{nameof(action)} '{action}' could not be prased");
        }

        private string GetSettlementBuilderAction(SettlementBuilderAction action)
        {
            switch (action)
            {
                case SettlementBuilderAction.Add:
                    return "AddSettlement";
                case SettlementBuilderAction.Remove:
                    return "RemoveSettlement";
            }

            throw new ArgumentException($"{nameof(action)} '{action}' could not be prased");
        }
    }
}
