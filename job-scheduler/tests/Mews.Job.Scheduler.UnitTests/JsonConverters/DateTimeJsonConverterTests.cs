using System.Text.Json;
using Mews.Job.Scheduler.BuildingBlocks.Configuration.Serialization;

namespace Mews.Job.Scheduler.UnitTests.JsonConverters;

[TestFixture]
public sealed class DateTimeJsonConverterTests
{
    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        Converters = { new DateTimeJsonConverter() }
    };
    
    [Test]
    public void Serialize_DateTime_ReturnsCorrectJson()
    {
        // Arrange
        var value = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var result = JsonSerializer.Serialize(value, _serializerOptions);

        // Assert
        Assert.That(result, Is.EqualTo("\"2023-10-01T12:00:00.0000000Z\""));
    }
    
    [Test]
    public void Deserialize_ValidDateTimeJson_ReturnsCorrectDateTime()
    {
        // Arrange
        var json = "\"2023-10-01T12:00:00.0000000Z\"";

        // Act
        var result = JsonSerializer.Deserialize<DateTime>(json, _serializerOptions);

        // Assert
        Assert.That(result, Is.EqualTo(new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc)));
    }
}
