using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.NewtonsoftJson;
using Newtonsoft.Json;

namespace NetCorePal.Extensions.AspNetCore.UnitTests;

public class NewtonsoftDeletedTimeJsonConverterTest
{
    private readonly JsonSerializerSettings _settings = new()
    {
        Converters = { new NewtonsoftDeletedTimeJsonConverter() }
    };

    [Fact]
    public void Deserialize_Null_ReturnsNull()
    {
        const string json = "null";
        var result = JsonConvert.DeserializeObject<DeletedTime>(json, _settings);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("\"2023-01-01T12:00:00+08:00\"", "2023-01-01T12:00:00+08:00")]
    [InlineData("\"2023-01-01T04:00:00Z\"", "2023-01-01T04:00:00Z")]
    public void Deserialize_Iso8601String_ReturnsCorrectTime(string json, string expected)
    {
        var result = JsonConvert.DeserializeObject<DeletedTime>(json, _settings);
        Assert.Equal(DateTimeOffset.Parse(expected), result?.Value);
    }

    [Fact]
    public void Deserialize_DateTime_ReturnsCorrectOffset()
    {
        const string json = "\"2023-01-01T12:00:00\"";
        var result = JsonConvert.DeserializeObject<DeletedTime>(json, _settings);
        Assert.Equal(DateTimeOffset.Parse("2023-01-01T12:00:00+00:00"), result?.Value);
    }

    [Fact]
    public void Deserialize_InvalidString_ThrowsException()
    {
        const string json = "\"invalid-date\"";
        var ex = Assert.Throws<JsonSerializationException>(() =>
            JsonConvert.DeserializeObject<DeletedTime>(json, _settings));
        Assert.Contains("无效的日期格式", ex.Message);
    }

    [Fact]
    public void Serialize_ValidTime_ReturnsIso8601()
    {
        var time = new DeletedTime(new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var json = JsonConvert.SerializeObject(time, _settings);
        Assert.Equal("\"2023-01-01T12:00:00+00:00\"", json);
    }

    [Fact]
    public void RoundTrip_ComplexObject_Works()
    {
        var original = new TestRecord(
            new DeletedTime(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            "Test"
        );

        var json = JsonConvert.SerializeObject(original, _settings);
        var result = JsonConvert.DeserializeObject<TestRecord>(json, _settings);

        Assert.NotNull(result);
        Assert.Equal(original.Time.Value, result!.Time.Value);
        Assert.Equal(original.Name, result.Name);
    }

    public record TestRecord(DeletedTime Time, string Name);
}