using NetCorePal.Extensions.AspNetCore.CommandLocks;
using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.AspNetCore.UnitTests;

public partial record OrderId : IInt64StronglyTypedId;

public class EntityIdExtensionsTests
{
    [Fact]
    public void One_Id_ToCommandLockSettings()
    {
        var orderId = new OrderId(10);
        Assert.Equal("10", orderId.ToString());
        var settings = orderId.ToCommandLockSettings(11);
        Assert.Equal("OrderId-10", settings.LockKey);
        Assert.Equal(11, settings.AcquireTimeout.TotalSeconds);
        Assert.Null(settings.LockKeys);
    }

    [Fact]
    public void Two_Id_ToCommandLockSettings()
    {
        var orderId = new OrderId(10);
        var orderId2 = new OrderId(20);

        List<OrderId> orderIds = new() { orderId, orderId2 };
        Assert.Equal("10", orderId.ToString());
        Assert.Equal("20", orderId2.ToString());
        var settings = orderIds.ToCommandLockSettings(11);
        Assert.Null(settings.LockKey);
        Assert.NotNull(settings.LockKeys);
        Assert.Equal(2, settings.LockKeys.Count);
        Assert.Equal("OrderId-10", settings.LockKeys[0]);
        Assert.Equal("OrderId-20", settings.LockKeys[1]);
        Assert.Equal(11, settings.AcquireTimeout.TotalSeconds);
    }
}