using NetCorePal.Extensions.AspNetCore.Json;
using NetCorePal.Extensions.Domain;
using Newtonsoft.Json;

namespace NetCorePal.Extensions.AspNetCore.UnitTests;

public partial record MyLongId : IInt64StronglyTypedId;

public partial record MyIntId : IInt32StronglyTypedId;

public partial record MyGuidId : IGuidStronglyTypedId;

public partial record MyStringId : IStringStronglyTypedId;

public class NewtonsoftEntityIdJsonConverterTests
{
    [Fact]
    public void CanConvertTest()
    {
        var converter = new NewtonsoftEntityIdJsonConverter();

        Assert.True(converter.CanConvert(typeof(MyLongId)));
        Assert.True(converter.CanConvert(typeof(MyIntId)));
        Assert.True(converter.CanConvert(typeof(MyGuidId)));
        Assert.True(converter.CanConvert(typeof(MyStringId)));
        Assert.False(converter.CanConvert(typeof(object)));
    }

    [Fact]
    public void Int64EntityTest()
    {
        JsonSerializerSettings settings = new();
        settings.Converters.Add(new NewtonsoftEntityIdJsonConverter());
        var id = new MyLongId(1);
        var json = JsonConvert.SerializeObject(id, settings);
        Assert.Equal("\"1\"", json);
        var id2 = JsonConvert.DeserializeObject<MyLongId>(json, settings);
        Assert.Equal(id, id2);
        var id3 = JsonConvert.DeserializeObject<MyLongId>("null", settings);
        Assert.Null(id3);
        var id4 = JsonConvert.DeserializeObject<MyLongId>("10", settings);
        Assert.NotNull(id4);
        Assert.Equal(10, id4.Id);
        
        Assert.Throws<FormatException>(() => JsonConvert.DeserializeObject<MyLongId>("\"\"", settings));
    }

    [Fact]
    public void Int32EntityTest()
    {
        JsonSerializerSettings settings = new();
        settings.Converters.Add(new NewtonsoftEntityIdJsonConverter());
        var id = new MyIntId(1);
        var json = JsonConvert.SerializeObject(id, settings);
        Assert.Equal("1", json);
        var id2 = JsonConvert.DeserializeObject<MyIntId>(json, settings);
        Assert.Equal(id, id2);
        var id3 = JsonConvert.DeserializeObject<MyIntId>("null", settings);
        Assert.Null(id3);
        var id4 = JsonConvert.DeserializeObject<MyIntId>("10", settings);
        Assert.NotNull(id4);
        Assert.Equal(10, id4.Id);

        Assert.Throws<FormatException>(() => JsonConvert.DeserializeObject<MyIntId>("\"\"", settings));
    }

    [Fact]
    public void GuidEntityTest()
    {
        JsonSerializerSettings settings = new();
        settings.Converters.Add(new NewtonsoftEntityIdJsonConverter());
        var id = new MyGuidId(Guid.NewGuid());
        var json = JsonConvert.SerializeObject(id, settings);
        Assert.Equal("\"" + id.Id.ToString() + "\"", json);
        var id2 = JsonConvert.DeserializeObject<MyGuidId>(json, settings);
        Assert.Equal(id, id2);
        var id3 = JsonConvert.DeserializeObject<MyGuidId>("null", settings);
        Assert.Null(id3);

        Assert.Throws<FormatException>(() => JsonConvert.DeserializeObject<MyGuidId>("\"\"", settings));
    }

    [Fact]
    public void StringEntityTest()
    {
        JsonSerializerSettings settings = new();
        settings.Converters.Add(new NewtonsoftEntityIdJsonConverter());
        var id = new MyStringId("1");
        var json = JsonConvert.SerializeObject(id, settings);
        Assert.Equal("\"1\"", json);
        var id2 = JsonConvert.DeserializeObject<MyStringId>(json, settings);
        Assert.Equal(id, id2);
        var id3 = JsonConvert.DeserializeObject<MyStringId>("null", settings);
        Assert.Null(id3);
        var id4 = JsonConvert.DeserializeObject<MyStringId>("\"\"", settings);
        Assert.NotNull(id4);
        Assert.Equal("", id4.Id);
    }
}