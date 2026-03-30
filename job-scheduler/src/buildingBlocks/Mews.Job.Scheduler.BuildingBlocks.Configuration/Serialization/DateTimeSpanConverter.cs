using System.Text.Json;
using System.Text.Json.Serialization;
using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;

namespace Mews.Job.Scheduler.BuildingBlocks.Configuration.Serialization;

public sealed class DateTimeSpanJsonConverter : JsonConverter<DateTimeSpan>
{
    public override DateTimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        var parsed = value?.ToDateTimeSpan();

        if (parsed.HasValue)
        {
            return parsed.Value;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, DateTimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
