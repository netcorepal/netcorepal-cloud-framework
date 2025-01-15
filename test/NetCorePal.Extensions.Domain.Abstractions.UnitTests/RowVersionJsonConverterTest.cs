using System.Text.Json;
using NetCorePal.Extensions.Domain.Json;

namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests;

public class RowVersionJsonConverterTest
{
    [Fact]
    public void Test_Serialize_Deserialize()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new RowVersionJsonConverter());
        var rowVersion = new RowVersion(2025);
        var json = JsonSerializer.Serialize(rowVersion, options);
        Assert.Equal("2025", json);
        var obj = JsonSerializer.Deserialize<RowVersion>(json, options);
        Assert.NotNull(obj);
        Assert.Equal(2025, obj.VersionNumber);
    }

    [Fact]
    public void TestNull()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new RowVersionJsonConverter());
        var json = JsonSerializer.Serialize<UpdateTime?>(null, options);
        Assert.Equal("null", json);
        var obj = JsonSerializer.Deserialize<UpdateTime?>(json, options);
        Assert.Null(obj);
    }

    [Fact]
    public void TestDto()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new RowVersionJsonConverter());
        var dto = new Dto
        {
            Name = "test",
            Version = new RowVersion(2025)
        };
        var json = JsonSerializer.Serialize(dto, options);
        Assert.Equal("{\"Name\":\"test\",\"Version\":2025}", json);
        var obj = JsonSerializer.Deserialize<Dto>(json, options);
        Assert.NotNull(obj);
        Assert.Equal(obj.Name, dto.Name);
        Assert.Equal(2025, obj.Version!.VersionNumber);

        dto = new Dto();

        json = JsonSerializer.Serialize(dto, options);
        Assert.Equal("{\"Name\":null,\"Version\":null}", json);
        obj = JsonSerializer.Deserialize<Dto>(json, options);
        Assert.NotNull(obj);
        Assert.Null(obj.Name);
        Assert.Null(obj.Version);
    }


    class Dto
    {
        public string? Name { get; set; }
        public RowVersion? Version { get; set; }
    }
}