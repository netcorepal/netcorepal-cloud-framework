using NetCorePal.Extensions.Domain.Json;
using System.Text.Json;

namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests
{
    public class EntityIdJsonConverterTests
    {
        [Fact]
        public void Serialize_Deserialize_Test()
        {
            JsonSerializerOptions options = new();
            options.Converters.Add(new EntityIdJsonConverterFactory());

            var id = JsonSerializer.Deserialize<OrderId1>("\"12\"", options);
            Assert.NotNull(id);
            Assert.True(id.Id == 12);
            var json = JsonSerializer.Serialize(id, options);
            Assert.Equal("\"12\"", json);
            id = JsonSerializer.Deserialize<OrderId1>("16", options);
            Assert.NotNull(id);
            Assert.Equal(16, id.Id);


            var id2 = JsonSerializer.Deserialize<OrderId2>("\"2\"", options);
            Assert.NotNull(id2);
            Assert.Equal(2, id2.Id);
            json = JsonSerializer.Serialize(id2, options);
            Assert.Equal("2", json);
            id2 = JsonSerializer.Deserialize<OrderId2>("5", options);
            Assert.NotNull(id2);
            Assert.Equal(5, id2.Id);

            var id3 = JsonSerializer.Deserialize<OrderId3>("\"abc\"", options);
            Assert.NotNull(id3);
            Assert.Equal("abc", id3.Id);
            json = JsonSerializer.Serialize(id3, options);
            Assert.Equal("\"abc\"", json);
            
            var id4 = JsonSerializer.Deserialize<OrderId4>("\"0f8a7a4d-4a3d-4d3d-8d3a-3d4a7a0f8a7a\"", options);
            Assert.NotNull(id4);
            Assert.Equal("0f8a7a4d-4a3d-4d3d-8d3a-3d4a7a0f8a7a", id4.Id.ToString());
            json = JsonSerializer.Serialize(id4, options);
            Assert.Equal("\"0f8a7a4d-4a3d-4d3d-8d3a-3d4a7a0f8a7a\"", json);
            
        }
    }
}