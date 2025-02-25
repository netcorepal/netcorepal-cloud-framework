using System.Text.Json;
using NetCorePal.Extensions.Domain.Json;

namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests;

public class DeletedTimeJsonConverterTest
{
    private readonly JsonSerializerOptions _options;

    public DeletedTimeJsonConverterTest()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new DeletedTimeJsonConverter());
    }

    [Fact]
    public void Deserialize_Null_ReturnsNull()
    {
        const string json = "null";
        var result = JsonSerializer.Deserialize<DeletedTime>(json, _options);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("\"2023-01-01T12:00:00+08:00\"", "2023-01-01T12:00:00+08:00")]
    [InlineData("\"2023-01-01T04:00:00Z\"", "2023-01-01T04:00:00Z")]
    public void Deserialize_ValidIso8601_ReturnsCorrectTime(string json, string expected)
    {
        var result = JsonSerializer.Deserialize<DeletedTime>(json, _options);
        Assert.Equal(DateTimeOffset.Parse(expected), result?.Value);
    }

    [Fact]
    public void Deserialize_InvalidString_ThrowsException()
    {
        const string json = "\"invalid-date\"";
        var ex = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<DeletedTime>(json, _options));
        Assert.Contains("无效的日期时间格式", ex.Message);
    }

    [Fact]
    public void Deserialize_UnsupportedType_ThrowsException()
    {
        const string json = "true";
        var ex = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<DeletedTime>(json, _options));
        Assert.Contains("意外的Token类型", ex.Message);
    }

    [Fact]
    public void Serialize_ValidTime_ReturnsIso8601()
    {
        var time = new DeletedTime(new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var json = JsonSerializer.Serialize(time, _options);
        Assert.Equal("\"2023-01-01T12:00:00+00:00\"", json);
    }

    [Fact]
    public void Serialize_Null_ReturnsNull()
    {
        DeletedTime? time = null;
        var json = JsonSerializer.Serialize(time, _options);
        Assert.Equal("null", json);
    }

    [Fact]
    public void RoundTrip_ComplexObject_Works()
    {
        var original = new TestRecord(
            new DeletedTime(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            "Test"
        );

        var json = JsonSerializer.Serialize(original, _options);
        var result = JsonSerializer.Deserialize<TestRecord>(json, _options);

        Assert.NotNull(result);
        Assert.Equal(original.Time.Value, result.Time.Value);
        Assert.Equal(original.Name, result.Name);
    }

    public record TestRecord(DeletedTime Time, string Name);
}