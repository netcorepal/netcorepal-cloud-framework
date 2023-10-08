using NetCorePal.Extensions.Domain.Json;
using System.Text.Json;

namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests
{
    public class EntityIdJsonConverterTests
    {
        [Fact]
        public void Serialize_Deserialize_Test()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new EntityIdJsonConverterFactory());

            var id = JsonSerializer.Deserialize<OrderId1>("\"12\"", options);
            Assert.NotNull(id);
            Assert.True(id.Id == 12);
            id = JsonSerializer.Deserialize<OrderId1>("\"13\"", options);
            var id2 = new OrderId2(2);
            var json = JsonSerializer.Serialize(id2, options);
            Assert.Equal("\"2\"", json);
            options = new JsonSerializerOptions();
            options.Converters.Add(new EntityIdJsonConverterFactory());
            var json2 = JsonSerializer.Serialize(id2, options);
        }
    }

}
