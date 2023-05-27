using NetCorePal.Extensions.Domain;
using NUnit.Framework;

namespace NetCorePal.AspNetCore.Gateway.UnitTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void Test1()
        {
            EntityId id = 10;
            Assert.AreEqual(10L, id);


            id += 20;
            Assert.AreEqual(30L, id);
            Assert.Pass();
        }
    }
}