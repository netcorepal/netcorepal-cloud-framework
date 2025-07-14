using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents;

/// <summary>
/// 订单创建领域事件
/// </summary>
public class OrderCreatedDomainEvent : IDomainEvent
{
    public Order Order { get; }

    public OrderCreatedDomainEvent(Order order)
    {
        Order = order;
    }
}

/// <summary>
/// 订单支付领域事件
/// </summary>
public class OrderPaidDomainEvent : IDomainEvent
{
    public Order Order { get; }

    public OrderPaidDomainEvent(Order order)
    {
        Order = order;
    }
}

/// <summary>
/// 订单名称变更领域事件
/// </summary>
public class OrderNameChangedDomainEvent : IDomainEvent
{
    public Order Order { get; }
    public string NewName { get; }

    public OrderNameChangedDomainEvent(Order order, string newName)
    {
        Order = order;
        NewName = newName;
    }
}

/// <summary>
/// 订单删除领域事件
/// </summary>
public class OrderDeletedDomainEvent : IDomainEvent
{
    public Order Order { get; }

    public OrderDeletedDomainEvent(Order order)
    {
        Order = order;
    }
}

/// <summary>
/// 用户创建领域事件
/// </summary>
public class UserCreatedDomainEvent : IDomainEvent
{
    public User User { get; }

    public UserCreatedDomainEvent(User user)
    {
        User = user;
    }
}

/// <summary>
/// 用户激活领域事件
/// </summary>
public class UserActivatedDomainEvent : IDomainEvent
{
    public User User { get; }

    public UserActivatedDomainEvent(User user)
    {
        User = user;
    }
}

/// <summary>
/// 用户禁用领域事件
/// </summary>
public class UserDeactivatedDomainEvent : IDomainEvent
{
    public User User { get; }

    public UserDeactivatedDomainEvent(User user)
    {
        User = user;
    }
}

/// <summary>
/// 订单项添加领域事件
/// </summary>
public class OrderItemAddedDomainEvent : IDomainEvent
{
    public OrderItem OrderItem { get; }

    public OrderItemAddedDomainEvent(OrderItem orderItem)
    {
        OrderItem = orderItem;
    }
}

/// <summary>
/// 订单项数量变更领域事件
/// </summary>
public class OrderItemQuantityChangedDomainEvent : IDomainEvent
{
    public OrderItem OrderItem { get; }
    public int OldQuantity { get; }
    public int NewQuantity { get; }

    public OrderItemQuantityChangedDomainEvent(OrderItem orderItem, int oldQuantity, int newQuantity)
    {
        OrderItem = orderItem;
        OldQuantity = oldQuantity;
        NewQuantity = newQuantity;
    }
}

/// <summary>
/// 订单项价格变更领域事件
/// </summary>
public class OrderItemPriceChangedDomainEvent : IDomainEvent
{
    public OrderItem OrderItem { get; }
    public decimal OldUnitPrice { get; }
    public decimal NewUnitPrice { get; }

    public OrderItemPriceChangedDomainEvent(OrderItem orderItem, decimal oldUnitPrice, decimal newUnitPrice)
    {
        OrderItem = orderItem;
        OldUnitPrice = oldUnitPrice;
        NewUnitPrice = newUnitPrice;
    }
}

/// <summary>
/// 订单项移除领域事件
/// </summary>
public class OrderItemRemovedDomainEvent : IDomainEvent
{
    public OrderItem OrderItem { get; }

    public OrderItemRemovedDomainEvent(OrderItem orderItem)
    {
        OrderItem = orderItem;
    }
}
