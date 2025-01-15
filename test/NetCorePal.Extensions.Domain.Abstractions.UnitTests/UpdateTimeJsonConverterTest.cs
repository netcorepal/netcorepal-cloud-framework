using NetCorePal.Extensions.Domain.Json;
using System.Text.Json;

namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests;

public class UpdateTimeJsonConverterTest
{
    [Fact]
    public void Test_Offset_0()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new UpdateTimeJsonConverter());
        var time = new DateTimeOffset(2025, 1, 14, 21, 10, 5, TimeSpan.Zero);
        var json = JsonSerializer.Serialize(new UpdateTime(time), options);
        Assert.Equal("\"2025-01-14T21:10:05+00:00\"", json);
        var obj = JsonSerializer.Deserialize<UpdateTime>(json, options);
        Assert.NotNull(obj);
        Assert.Equal(obj.Value, time);
    }


    [Fact]
    public void Test_offset_8()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new UpdateTimeJsonConverter());
        var time = new DateTimeOffset(2025, 1, 14, 21, 10, 5, TimeSpan.FromHours(8));
        var json = JsonSerializer.Serialize(new UpdateTime(time), options);
        Assert.Equal("\"2025-01-14T21:10:05+08:00\"", json);
        var obj = JsonSerializer.Deserialize<UpdateTime>(json, options);
        Assert.NotNull(obj);
        Assert.Equal(obj.Value, time);
    }

    [Fact]
    public void TestNull()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new UpdateTimeJsonConverter());
        var json = JsonSerializer.Serialize<UpdateTime?>(null, options);
        Assert.Equal("null", json);
        var obj = JsonSerializer.Deserialize<UpdateTime?>(json, options);
        Assert.Null(obj);
    }

    [Fact]
    public void TestDto()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new UpdateTimeJsonConverter());
        var time = new DateTimeOffset(2025, 1, 14, 21, 10, 5, TimeSpan.Zero);
        var dto = new Dto
        {
            Name = "test",
            UpdateTime = new UpdateTime(time)
        };
        var json = JsonSerializer.Serialize(dto, options);
        Assert.Equal("{\"Name\":\"test\",\"UpdateTime\":\"2025-01-14T21:10:05+00:00\"}", json);
        var obj = JsonSerializer.Deserialize<Dto>(json, options);
        Assert.NotNull(obj);
        Assert.Equal(obj.Name, dto.Name);
        Assert.Equal(obj.UpdateTime?.Value, time);

        dto = new Dto();
        
        json = JsonSerializer.Serialize(dto, options);
        Assert.Equal("{\"Name\":null,\"UpdateTime\":null}", json);
        obj = JsonSerializer.Deserialize<Dto>(json, options);
        Assert.NotNull(obj);
        Assert.Null(obj.Name);
        Assert.Null(obj.UpdateTime);
        
    }


    class Dto
    {
        public string? Name { get; set; }
        public UpdateTime? UpdateTime { get; set; }
    }
}