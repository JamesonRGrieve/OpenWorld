using System;
using System.Collections.Generic;
using OpenWorld.Shared.Enums;

namespace OpenWorld.Shared.Networking.Packets.Faction
{
    public abstract class FactionManagementPacketBase : PacketBase
    {
        public override PacketType Type => PacketType.FactionManagement;

        public abstract FactionManagementType ManagementType { get; }

        public override string GetData()
        {
            var splits = new List<string>() { "FactionManagement", this.GetManagementType(this.ManagementType) };
            var additionalData = this.GetAdditionalData();

            if (additionalData != null && additionalData.Count != 0)
            {
                splits.AddRange(additionalData);
            }

            return this.BuildData(splits);
        }

        private string GetManagementType(FactionManagementType managementType)
        {
            switch (managementType)
            {
                case FactionManagementType.Details:
                    return "Details";
            }

            throw new ArgumentException($"{nameof(managementType)} '{managementType}' could not be prased");
        }

        protected abstract List<string> GetAdditionalData();
    }
}
