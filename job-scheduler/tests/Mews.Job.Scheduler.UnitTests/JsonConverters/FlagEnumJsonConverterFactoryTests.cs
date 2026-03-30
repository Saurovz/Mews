using System.Text.Json;
using Mews.Job.Scheduler.BuildingBlocks.Configuration.Serialization;

namespace Mews.Job.Scheduler.UnitTests.JsonConverters;

[Flags]
public enum TestFlags
{
    None = 0,
    Flag1 = 1 << 0,
    Flag2 = 1 << 1,
    Flag3 = 1 << 2
}

[TestFixture]
public sealed class FlagEnumJsonConverterTests
{
    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        Converters = { new FlagEnumJsonConverter() }
    };
    
    [Test]
    public void Serialize_NoneFlag_ReturnsCorrectJson()
    {
        // Arrange
        var value = TestFlags.None;

        // Act
        var result = JsonSerializer.Serialize(value, _serializerOptions);

        // Assert
        Assert.That(result, Is.EqualTo("[\"None\"]"));
    }

    [Test]
    public void Serialize_FlagEnum_ReturnsCorrectJson()
    {
        // Arrange
        var value = TestFlags.Flag1 | TestFlags.Flag2 | TestFlags.Flag3;

        // Act
        var result = JsonSerializer.Serialize(value, _serializerOptions);

        // Assert
        Assert.That(result, Is.EqualTo("[\"Flag1\",\"Flag2\",\"Flag3\"]"));
    }
    
    [Test]
    public void Deserialize_NoneJsonValue_ReturnsCorrectFlagEnum()
    {
        // Arrange
        var json = "[\"None\"]";

        // Act
        var result = JsonSerializer.Deserialize<TestFlags>(json, _serializerOptions);

        // Assert
        Assert.That(result, Is.EqualTo(TestFlags.None));
    }
    
    [Test]
    public void Deserialize_JsonValues_ReturnsCorrectFlagEnum()
    {
        // Arrange
        var json = "[\"Flag1\",\"Flag2\",\"Flag3\"]";

        // Act
        var result = JsonSerializer.Deserialize<TestFlags>(json, _serializerOptions);

        // Assert
        Assert.That(result, Is.EqualTo(TestFlags.Flag1 | TestFlags.Flag2 | TestFlags.Flag3));
    }
    
    [Test]
    public void Deserialize_InvalidValueJson_ThrowsJsonException()
    {
        // Arrange
        var json = "[\"InvalidFlag\"]";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TestFlags>(json, _serializerOptions));
    }
}
