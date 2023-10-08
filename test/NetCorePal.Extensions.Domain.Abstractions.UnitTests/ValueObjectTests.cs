namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests;

public class ValueObjectTests
{
    class ValueObject1(int id, string name = "abc") : ValueObject
    {
        public int Id { get; } = id;

        public string Name { get; } = name;

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
            yield return Name;
        }
    }

    [Fact]
    public void ValueObject_EqualOperator_Test()
    {
        var valueObject1 = new ValueObject1(1);
        var valueObject2 = new ValueObject1(1);
        var valueObject3 = new ValueObject1(1, "efg");
        var valueObject4 = new ValueObject1(2);
        Assert.True(valueObject1.Equals(valueObject2));
        Assert.False(valueObject1.Equals(valueObject3));
        Assert.False(valueObject1.Equals(valueObject4));
    }
}