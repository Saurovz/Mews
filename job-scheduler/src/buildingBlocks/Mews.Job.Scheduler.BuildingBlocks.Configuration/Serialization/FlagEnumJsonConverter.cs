using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mews.Job.Scheduler.BuildingBlocks.Configuration.Serialization;

public sealed class FlagEnumJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsFlagEnum();
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converter = (JsonConverter)Activator.CreateInstance(
            type: typeof(FlagEnumJsonConverter<>).MakeGenericType(typeToConvert),
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: null,
            culture: null
        )!;

        return converter;
    }
}

public sealed class FlagEnumJsonConverter<TFlagEnum> : JsonConverter<TFlagEnum>
    where TFlagEnum : struct, Enum
{
    private readonly Type _jsonRepresentationType = typeof(IEnumerable<string>);
    private readonly JsonConverter<IEnumerable<string>> _enumerableOfStringConverter = (JsonConverter<IEnumerable<string>>)JsonSerializerOptions.Default.GetConverter(typeof(IEnumerable<string>));

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsFlagEnum();
    }

    public override TFlagEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var values = _enumerableOfStringConverter.Read(ref reader, _jsonRepresentationType, options)?.ToList();

        if (values is null)
        {
            throw new JsonException();
        }

        var sum = 0L;
        foreach (var value in values)
        {
            var parsed = Enum.TryParse<TFlagEnum>(value, out var flag);
            if (!parsed)
            {
                throw new JsonException();
            }

            sum |= flag.ToInt64();
        }
        return sum.ToEnum<TFlagEnum>();
    }

    public override void Write(Utf8JsonWriter writer, TFlagEnum value, JsonSerializerOptions options)
    {
        var enumStrings = value.ToString().Split(", ");

        _enumerableOfStringConverter.Write(writer, enumStrings, options);
    }
}
