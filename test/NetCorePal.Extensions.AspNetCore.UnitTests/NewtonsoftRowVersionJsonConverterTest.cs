using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.NewtonsoftJson;
using Newtonsoft.Json;

namespace NetCorePal.Extensions.AspNetCore.UnitTests;

public class NewtonsoftRowVersionJsonConverterTest
{
    [Fact]
    public void CanConvertTest()
    {
        var n = DateTimeOffset.UtcNow;
        var nn = DateTimeOffset.Now;


        var converter = new NewtonsoftRowVersionJsonConverter();
        JsonSerializerSettings settings = new();
        settings.Converters.Add(converter);

        Assert.True(converter.CanConvert(typeof(RowVersion)));
        Assert.False(converter.CanConvert(typeof(object)));
    }

    [Fact]
    public void Test_SerializeObject_DeserializeObject()
    {
        JsonSerializerSettings settings = new();
        settings.Converters.Add(new NewtonsoftRowVersionJsonConverter());
        var json = JsonConvert.SerializeObject(new RowVersion(2025), settings);
        Assert.Equal("2025", json);
        var obj = JsonConvert.DeserializeObject<RowVersion>(json, settings);
        Assert.NotNull(obj);
        Assert.Equal(2025, obj.VersionNumber);
    }

    [Fact]
    public void TestNull()
    {
        JsonSerializerSettings settings = new();
        settings.Converters.Add(new NewtonsoftRowVersionJsonConverter());
        var json = JsonConvert.SerializeObject(null, typeof(RowVersion), settings);
        Assert.Equal("null", json);
        var obj = JsonConvert.DeserializeObject<RowVersion?>(json, settings);
        Assert.Null(obj);
    }

    [Fact]
    public void TestDto()
    {
        JsonSerializerSettings settings = new();
        settings.Converters.Add(new NewtonsoftRowVersionJsonConverter());
        var dto = new Dto
        {
            Name = "test",
            Version = new RowVersion(2025)
        };
        var json = JsonConvert.SerializeObject(dto, settings);
        Assert.Equal("{\"Name\":\"test\",\"Version\":2025}", json);
        var obj = JsonConvert.DeserializeObject<Dto>(json, settings);
        Assert.NotNull(obj);
        Assert.NotNull(obj.Version);
        Assert.Equal(2025, obj.Version.VersionNumber);
    }

    public class Dto
    {
        public string? Name { get; set; }
        public RowVersion? Version { get; set; }
    }
}