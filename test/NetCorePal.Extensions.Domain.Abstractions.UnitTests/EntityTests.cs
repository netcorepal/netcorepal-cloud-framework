namespace NetCorePal.Extensions.Domain.Abstractions.UnitTests;

public class EntityTests
{
    [Fact]
    public void SameId_Should_Be_Equals()
    {
        var order1 = new Order1(1);
        var order2 = new Order1(1);
        Assert.True(order1.Equals(order2));
    }

    [Fact]
    public void DifferentId_Should_Not_Equals()
    {
        var order1 = new Order1(1);
        var order2 = new Order1(2);
        Assert.False(order1.Equals(order2));
    }

    [Fact]
    public void SameId_Should_Be_Not_Equals_With_Operator()
    {
        var order1 = new Order1(1);
        var order2 = new Order1(1);
        Assert.False(order1 == order2);
    }

    [Fact]
    public void DifferentId_Should_Not_Equals_With_Operator()
    {
        var order1 = new Order1(1);
        var order2 = new Order1(2);
        Assert.False(order1 == order2);
    }

    [Fact]
    public void No_Id_Should_Not_Equals()
    {
        var order1 = new Order1();
        var order2 = new Order1();
        Assert.False(order1.Equals(order2));
    }

    [Fact]
    public void No_Id_Should_Not_Equals_With_Operator()
    {
        var order1 = new Order1();
        var order2 = new Order1();
        Assert.False(order1 == order2);
    }


    [Fact]
    public void No_Id_Is_Transient_Should_True()
    {
        var order1 = new Order1();
        Assert.True(order1.IsTransient());
    }

    [Fact]
    public void Id_Is_Transient_Should_False()
    {
        var order1 = new Order1(1);
        Assert.False(order1.IsTransient());
    }


    [Fact]
    public void GetHashCode_Should_Not_Throw()
    {
        var order1 = new Order1(1);
        var hashCode = order1.GetHashCode();
        Assert.NotEqual(0, hashCode);
    }

    [Fact]
    public void GetHashCode_Should_Not_Throw_With_No_Id()
    {
        var order1 = new Order1();
        var hashCode = order1.GetHashCode();
        Assert.NotEqual(0, hashCode);
    }
    
    [Fact]
    public void GetHashCode_Should_Not_Equal_With_Different_Id()
    {
        var order1 = new Order1(1);
        var order2 = new Order1(2);
        Assert.NotEqual(order1.GetHashCode(), order2.GetHashCode());
    }
    
    [Fact]
    public void GetHashCode_Should_Not_Equal_With_Different_With_Id_And_No_Id()
    {
        var order1 = new Order1(1);
        var order2 = new Order1();
        Assert.NotEqual(order1.GetHashCode(), order2.GetHashCode());
    }
    
    
    [Fact]
    public void GetHashCode_Should_Equal_Get_Two_Times_With_Id()
    {
        var order1 = new Order1(1);
        Assert.Equal(order1.GetHashCode(), order1.GetHashCode());
    }
    
    [Fact]
    public void GetHashCode_Should_Equal_Get_Two_Times_With_No_Id()
    {
        var order1 = new Order1();
        Assert.Equal(order1.GetHashCode(), order1.GetHashCode());
    }
    
    public class Order1 : Entity<OrderId1>
    {
        public Order1()
        {
        }

        public Order1(OrderId1 id)
        {
            Id = id;
        }
    }


    public class Order2 : Entity<OrderId2>
    {
        public Order2()
        {
        }

        public Order2(OrderId2 id)
        {
            Id = id;
        }
    }
}