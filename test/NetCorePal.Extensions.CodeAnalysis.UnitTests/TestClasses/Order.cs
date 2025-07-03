using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

/// <summary>
/// 订单ID强类型ID
/// </summary>
public partial record OrderId : IGuidStronglyTypedId;

/// <summary>
/// 订单聚合根
/// </summary>
public class Order : Entity<OrderId>, IAggregateRoot
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public bool IsPaid { get; private set; }

    protected Order()
    {
        Name = string.Empty;
    }

    public Order(OrderId id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
        IsPaid = false;
        
        // 发出领域事件
        AddDomainEvent(new OrderCreatedDomainEvent(this));
    }

    /// <summary>
    /// 标记订单为已支付
    /// </summary>
    public void MarkAsPaid()
    {
        IsPaid = true;
        AddDomainEvent(new OrderPaidDomainEvent(this));
    }

    /// <summary>
    /// 更改订单名称
    /// </summary>
    /// <param name="newName"></param>
    public void ChangeName(string newName)
    {
        Name = newName;
        AddDomainEvent(new OrderNameChangedDomainEvent(this, newName));
    }

    /// <summary>
    /// 软删除订单
    /// </summary>
    public void SoftDelete()
    {
        AddDomainEvent(new OrderDeletedDomainEvent(this));
    }
}
