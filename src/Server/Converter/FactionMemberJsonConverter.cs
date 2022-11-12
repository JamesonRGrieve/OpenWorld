using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenWorld.Shared.Enums;

namespace OpenWorldServer.Converter
{
    internal class FactionMemberJsonConverter : JsonConverter<Dictionary<Guid, FactionRank>>
    {
        public override Dictionary<Guid, FactionRank> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dic = new Dictionary<Guid, FactionRank>();

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                // Get the key.
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var rawKey = reader.GetString();
                if (!Guid.TryParse(rawKey, out var key))
                {
                    throw new JsonException($"Unable to convert \"{rawKey}\" to GUID.");
                }

                reader.Read();
                // Get the Value.
                if (reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException();
                }

                var rawValue = reader.GetString();

                // For performance, parse with ignoreCase:false first.
                if (!Enum.TryParse(rawValue, ignoreCase: false, out FactionRank value) &&
                    !Enum.TryParse(rawValue, ignoreCase: true, out value))
                {
                    throw new JsonException($"Unable to convert \"{rawValue}\" to Enum \"{nameof(FactionRank)}\".");
                }

                dic.Add(key, value);
            }

            return dic;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<Guid, FactionRank> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var item in value)
            {
                writer.WritePropertyName(item.Key.ToString());
                writer.WriteStringValue(item.Value.ToString());
            }

            writer.WriteEndObject();
        }
    }
}
