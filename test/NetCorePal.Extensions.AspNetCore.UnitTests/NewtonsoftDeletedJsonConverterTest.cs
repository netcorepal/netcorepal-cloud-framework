using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.NewtonsoftJson;
using Newtonsoft.Json;

namespace NetCorePal.Extensions.AspNetCore.UnitTests;

public class NewtonsoftDeletedJsonConverterTest
{
    private readonly JsonSerializerSettings _settings = new()
    {
        Converters = { new NewtonsoftDeletedJsonConverter() }
    };

    [Fact]
    public void Deserialize_Null_ReturnsNull()
    {
        const string json = "null";
        var result = JsonConvert.DeserializeObject<Deleted>(json, _settings);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("\"true\"", true)]
    [InlineData("\"false\"", false)]
    public void Deserialize_ValidValues_ReturnsCorrectDeleted(string json, bool expected)
    {
        var result = JsonConvert.DeserializeObject<Deleted>(json, _settings);
        Assert.Equal(expected, result?.Value);
    }

    [Fact]
    public void Deserialize_InvalidString_ThrowsException()
    {
        const string json = "\"invalid\"";
        var ex = Assert.Throws<JsonSerializationException>(() =>
            JsonConvert.DeserializeObject<Deleted>(json, _settings));
        Assert.Contains("无效的布尔字符串值", ex.Message);
    }

    [Fact]
    public void Deserialize_UnsupportedType_ThrowsException()
    {
        const string json = "123.45";
        var ex = Assert.Throws<JsonSerializationException>(() =>
            JsonConvert.DeserializeObject<Deleted>(json, _settings));
        Assert.Contains("意外的Token类型", ex.Message);
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void Serialize_Deleted_ReturnsCorrectValue(bool input, string expectedJson)
    {
        var deleted = new Deleted(input);
        var json = JsonConvert.SerializeObject(deleted, _settings);
        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void Serialize_Null_ReturnsNull()
    {
        Deleted? deleted = null;
        var json = JsonConvert.SerializeObject(deleted, _settings);
        Assert.Equal("null", json);
    }

    [Theory]
    [InlineData("\"True\"", true)]
    [InlineData("\"FALSE\"", false)]
    public void Deserialize_CaseInsensitive_Works(string json, bool expected)
    {
        var result = JsonConvert.DeserializeObject<Deleted>(json, _settings);
        Assert.Equal(expected, result?.Value);
    }

    [Fact]
    public void Serialize_ComplexObject_Works()
    {
        var testObj = new TestRecord(new Deleted(true));
        var json = JsonConvert.SerializeObject(testObj, _settings);
        var result = JsonConvert.DeserializeObject<TestRecord>(json, _settings);

        Assert.NotNull(result);
        Assert.True(result.Deleted.Value);
    }

    public record TestRecord(Deleted Deleted);
}