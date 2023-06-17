using NetCorePal.Extensions.Domain.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static NetCorePal.Extensions.Domain.Abstractions.UnitTests.EntityIdTypeConverterTests;

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

            var id2 = new OrderId2(2);
            var json = JsonSerializer.Serialize(id2, options);
            Assert.Equal("\"2\"", json);
        }
    }

}
