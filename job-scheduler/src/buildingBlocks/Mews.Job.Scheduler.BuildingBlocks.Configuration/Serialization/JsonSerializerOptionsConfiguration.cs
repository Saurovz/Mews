using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mews.Job.Scheduler.BuildingBlocks.Configuration.Serialization;

public static class JsonSerializerOptionsConfiguration
{
    public static void Configure(JsonSerializerOptions serializerOptions)
    {
        // Order of enum converters is important, because we want to check for flag serialization/deserialization
        // before fallback to regular enum converter.
        serializerOptions.Converters.Add(new FlagEnumJsonConverter());
        serializerOptions.Converters.Add(new JsonStringEnumConverter());

        serializerOptions.Converters.Add(new DateTimeSpanJsonConverter());
        serializerOptions.Converters.Add(new DateTimeJsonConverter());
    }
}
