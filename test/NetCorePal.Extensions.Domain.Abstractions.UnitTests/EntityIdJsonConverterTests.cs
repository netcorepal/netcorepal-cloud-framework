using NetCorePal.Extensions.Domain.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests
{
    public class EntityIdJsonConverterTests
    {
        [Fact]
        public void Serialize_Deserialize_Test()
        {
            EntityId id = 10;
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();

            jsonSerializerOptions.Converters.Add(new EntityIdJsonConverter());

            var j = JsonSerializer.Serialize(id, jsonSerializerOptions);

            var idx = JsonSerializer.Deserialize<EntityId>(j, jsonSerializerOptions);

            var js = JsonSerializer.Serialize(new MyId(10), jsonSerializerOptions);
            Assert.Equal("\"10\"", js);

            var j2 = JsonSerializer.Serialize(new MyId(10), jsonSerializerOptions);
            Assert.Equal("\"10\"", j2);

            var j2x = JsonSerializer.Deserialize<MyId>(j2, jsonSerializerOptions);
            Assert.True(new MyId(10) == j2x);
        }
    }


    public class MyEntity
    {

    }

    public record MyId : EntityId<MyEntity>
    {
        public MyId(long id) : base(id) { }


        public override string ToString()
        {
            return base.ToString();
        }

    }
}
