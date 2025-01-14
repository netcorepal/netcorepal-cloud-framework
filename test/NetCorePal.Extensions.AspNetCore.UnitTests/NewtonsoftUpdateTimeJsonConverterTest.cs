using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.NewtonsoftJson;
using Newtonsoft.Json;

namespace NetCorePal.Extensions.AspNetCore.UnitTests;

public class NewtonsoftUpdateTimeJsonConverterTest
{
    [Fact]
    public void CanConvertTest()
    {
        var n = DateTimeOffset.UtcNow;
        var nn = DateTimeOffset.Now;
        
        
        var converter = new NewtonsoftUpdateTimeJsonConverter();
        JsonSerializerSettings settings = new();
        settings.Converters.Add(converter);

        Assert.True(converter.CanConvert(typeof(UpdateTime)));
        Assert.False(converter.CanConvert(typeof(object)));
    }

    [Fact]
    public void Test_Offset_0_Without_ParseHandling()
    {
        JsonSerializerSettings settings = new();
        //settings.DateParseHandling = DateParseHandling.DateTimeOffset;
        settings.Converters.Add(new NewtonsoftUpdateTimeJsonConverter());
        var time = new DateTimeOffset(2025, 1, 14, 21, 10, 5, TimeSpan.Zero);
        var json = JsonConvert.SerializeObject(new UpdateTime(time), settings);
        Assert.Equal("\"2025-01-14T21:10:05+00:00\"", json);
        var obj = JsonConvert.DeserializeObject<UpdateTime>(json, settings);
        Assert.NotNull(obj);
        Assert.Equal(obj.Value, time);
    }
    
    [Fact]
    public void Test_Offset_0_With_ParseHandling()
    {
        JsonSerializerSettings settings = new();
        settings.DateParseHandling = DateParseHandling.DateTimeOffset;
        settings.Converters.Add(new NewtonsoftUpdateTimeJsonConverter());
        var time = new DateTimeOffset(2025, 1, 14, 21, 10, 5, TimeSpan.Zero);
        var json = JsonConvert.SerializeObject(new UpdateTime(time), settings);
        Assert.Equal("\"2025-01-14T21:10:05+00:00\"", json);
        var obj = JsonConvert.DeserializeObject<UpdateTime>(json, settings);
        Assert.NotNull(obj);
        Assert.Equal(obj.Value, time);
    }

    [Fact]
    public void Test_offset_8_Without_ParseHandling()
    {
        JsonSerializerSettings settings = new();
        settings.Converters.Add(new NewtonsoftUpdateTimeJsonConverter());
        var time = new DateTimeOffset(2025, 1, 14, 21, 10, 5, TimeSpan.FromHours(8));
        var json = JsonConvert.SerializeObject(new UpdateTime(time), settings);
        Assert.Equal("\"2025-01-14T21:10:05+08:00\"", json);
        var obj = JsonConvert.DeserializeObject<UpdateTime>(json, settings);
        Assert.NotNull(obj);
        Assert.Equal(obj.Value, time);
    }
    
    [Fact]
    public void Test_offset_8_With_ParseHandling()
    {
        JsonSerializerSettings settings = new();
        settings.DateParseHandling = DateParseHandling.DateTimeOffset;
        settings.Converters.Add(new NewtonsoftUpdateTimeJsonConverter());
        var time = new DateTimeOffset(2025, 1, 14, 21, 10, 5, TimeSpan.FromHours(8));
        var json = JsonConvert.SerializeObject(new UpdateTime(time), settings);
        Assert.Equal("\"2025-01-14T21:10:05+08:00\"", json);
        var obj = JsonConvert.DeserializeObject<UpdateTime>(json, settings);
        Assert.NotNull(obj);
        Assert.Equal(obj.Value, time);
    }

    [Fact]
    public void TestNull()
    {
        JsonSerializerSettings settings = new();
        settings.Converters.Add(new NewtonsoftUpdateTimeJsonConverter());
        var json = JsonConvert.SerializeObject(null, typeof(UpdateTime), settings);
        Assert.Equal("null", json);
        var obj = JsonConvert.DeserializeObject<UpdateTime?>(json, settings);
        Assert.Null(obj);
    }

    [Fact]
    public void TestDto()
    {
        JsonSerializerSettings settings = new();
        settings.Converters.Add(new NewtonsoftUpdateTimeJsonConverter());
        var time = new DateTimeOffset(2025, 1, 14, 21, 10, 5, TimeSpan.Zero);
        var dto = new Dto
        {
            Name = "test",
            UpdateTime = new UpdateTime(time)
        };
        var json = JsonConvert.SerializeObject(dto, settings);
        Assert.Equal("{\"Name\":\"test\",\"UpdateTime\":\"2025-01-14T21:10:05+00:00\"}", json);
        var obj = JsonConvert.DeserializeObject<Dto>(json, settings);
        Assert.NotNull(obj);
    }

    public class Dto
    {
        public string? Name { get; set; }
        public UpdateTime? UpdateTime { get; set; }
    }
}