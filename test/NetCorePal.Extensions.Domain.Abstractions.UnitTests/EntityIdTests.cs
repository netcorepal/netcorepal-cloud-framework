namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests
{
    public class EntityIdTests
    {
        [Fact]
        public void Int64StronglyTypedId_Equal()
        {
            OrderId1 id1 = new OrderId1(1);
            OrderId1 id2 = new OrderId1(2);
            OrderId1 id3 = new OrderId1(1);
            Assert.True(id1.Equals(id3));
            Assert.True(id1 == id3);
            Assert.False(id1 == id2);
            Assert.False(id1.Equals(id2));
        }

        [Fact]
        public void Int32StronglyTypedId_Equal()
        {
            OrderId2 id1 = new OrderId2(1);
            OrderId2 id2 = new OrderId2(2);
            OrderId2 id3 = new OrderId2(1);
            Assert.True(id1.Equals(id3));
            Assert.True(id1 == id3);
            Assert.False(id1 == id2);
            Assert.False(id1.Equals(id2));
        }

        [Fact]
        public void StringStronglyTypedId_Equal()
        {
            OrderId3 id1 = new OrderId3("1");
            OrderId3 id2 = new OrderId3("2");
            OrderId3 id3 = new OrderId3("1");
            Assert.True(id1.Equals(id3));
            Assert.True(id1 == id3);
            Assert.False(id1 == id2);
            Assert.False(id1.Equals(id2));
        }

        [Fact]
        public void GuidStronglyTypedId_Equal()
        {
            OrderId4 id1 = new OrderId4(Guid.NewGuid());
            OrderId4 id2 = new OrderId4(Guid.NewGuid());
            OrderId4 id3 = new OrderId4(Guid.Parse(id1.Id.ToString()));
            Assert.True(id1.Equals(id3));
            Assert.True(id1 == id3);
            Assert.False(id1 == id2);
            Assert.False(id1.Equals(id2));
        }
    }
}