using System.Text.Json;
using Mews.Job.Scheduler.BuildingBlocks.Configuration.Serialization;
using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;

namespace Mews.Job.Scheduler.UnitTests.JsonConverters;

[TestFixture]
public sealed class DateTimeSpanJsonConverterTests
{
    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        Converters = { new DateTimeSpanJsonConverter() }
    };
    
    [Test]
    public void Serialize_DateTimeSpan_ReturnsCorrectJson()
    {
        // Arrange
        var value = new DateTimeSpan(months: 10, days: 1, hours: 12, minutes: 5, seconds: 20);

        // Act
        var result = JsonSerializer.Serialize(value, _serializerOptions);

        // Assert
        Assert.That(result, Is.EqualTo("\"10M1D12:5:20.0\""));
    }
    
    [Test]
    public void Deserialize_ValidJson_ReturnsCorrectDateTimeSpan()
    {
        // Arrange
        var json = "\"5M3D12:5:20.0\"";

        // Act
        var result = JsonSerializer.Deserialize<DateTimeSpan>(json, _serializerOptions);

        // Assert
        Assert.That(result, Is.EqualTo(new DateTimeSpan(months: 5, days: 3, hours: 12, minutes: 5, seconds: 20)));
    }
}
