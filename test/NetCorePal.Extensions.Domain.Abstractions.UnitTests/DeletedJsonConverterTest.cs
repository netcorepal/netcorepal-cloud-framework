using System.Text.Json;
using NetCorePal.Extensions.Domain.Json;

namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests;

public class DeletedJsonConverterTest
{
    private readonly JsonSerializerOptions _options;

    public DeletedJsonConverterTest()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new DeletedJsonConverter());
    }

    [Fact]
    public void Deserialize_Null_ReturnsNull()
    {
        const string json = "null";
        var result = JsonSerializer.Deserialize<Deleted>(json, _options);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("\"true\"", true)]
    [InlineData("\"false\"", false)]
    public void Deserialize_ValidValues_ReturnsCorrectDeleted(string json, bool expected)
    {
        var result = JsonSerializer.Deserialize<Deleted>(json, _options);
        Assert.Equal(expected, result?.Value);
    }

    [Fact]
    public void Deserialize_InvalidString_ThrowsException()
    {
        const string json = "\"invalid\"";
        var ex = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Deleted>(json, _options));
        Assert.Contains("无效的布尔字符串值", ex.Message);
    }

    [Fact]
    public void Deserialize_UnsupportedType_ThrowsException()
    {
        const string json = "123";
        var ex = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Deleted>(json, _options));
        Assert.Contains("意外的Token类型", ex.Message);
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void Serialize_Deleted_ReturnsCorrectValue(bool input, string expectedJson)
    {
        var deleted = new Deleted(input);
        var json = JsonSerializer.Serialize(deleted, _options);
        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void Serialize_Null_ReturnsNull()
    {
        Deleted? deleted = null;
        var json = JsonSerializer.Serialize(deleted, _options);
        Assert.Equal("null", json);
    }

    [Fact]
    public void Serialize_ComplexObject_WorksCorrectly()
    {
        var testObj = new TestRecord(new Deleted(true));
        var json = JsonSerializer.Serialize(testObj, _options);
        var result = JsonSerializer.Deserialize<TestRecord>(json, _options);

        Assert.NotNull(result);
        Assert.True(result.Deleted.Value);
    }

    public record TestRecord(Deleted Deleted);
}