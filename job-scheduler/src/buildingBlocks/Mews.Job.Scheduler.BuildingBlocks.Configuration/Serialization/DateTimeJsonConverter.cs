using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mews.Job.Scheduler.BuildingBlocks.Configuration.Serialization;

public sealed class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        var parsed = value?.FromIso8601String();

        if (parsed.HasValue)
        {
            return parsed.Value;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToIso8601String());
    }
}
