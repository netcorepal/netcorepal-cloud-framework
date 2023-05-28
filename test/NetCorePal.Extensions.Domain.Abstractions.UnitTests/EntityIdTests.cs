namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests
{
    public class EntityIdTests
    {
        [Fact]
        public void EntityId_Equal()
        {
            EntityId id = 10;
            Assert.True(10L == id);
            Assert.False(11L == id);
            EntityId id2 = 10;
            Assert.True(id == id2);
            EntityId id3 = 11;
            Assert.False(id == id3);
            long a = id;
            id += 20;
            Assert.True(30L == id);
        }
    }
}