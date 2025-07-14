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
    
    private readonly List<OrderItem> _orderItems = new();
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

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

    /// <summary>
    /// 创建默认订单的静态工厂方法
    /// </summary>
    /// <param name="id">订单ID</param>
    /// <returns>默认订单实例</returns>
    public static Order CreateDefault(OrderId id)
    {
        var order = new Order(id, "默认订单", 0);
        // 静态方法也可以发出领域事件
        order.AddDomainEvent(new OrderCreatedDomainEvent(order));
        return order;
    }

    /// <summary>
    /// 添加订单项
    /// </summary>
    /// <param name="orderItem">订单项</param>
    public void AddOrderItem(OrderItem orderItem)
    {
        _orderItems.Add(orderItem);
        
        // 明确发出订单项添加事件
        AddDomainEvent(new OrderItemAddedDomainEvent(orderItem));
        
        // 将子实体的其他领域事件添加到聚合根
        foreach (var domainEvent in orderItem.GetDomainEvents())
        {
            AddDomainEvent(domainEvent);
        }
        orderItem.ClearDomainEvents();
    }

    /// <summary>
    /// 移除订单项
    /// </summary>
    /// <param name="orderItemId">订单项ID</param>
    public void RemoveOrderItem(OrderItemId orderItemId)
    {
        var orderItem = _orderItems.FirstOrDefault(x => x.Id == orderItemId);
        if (orderItem != null)
        {
            orderItem.Remove();
            // 将子实体的领域事件添加到聚合根
            foreach (var domainEvent in orderItem.GetDomainEvents())
            {
                AddDomainEvent(domainEvent);
            }
            orderItem.ClearDomainEvents();
            _orderItems.Remove(orderItem);
        }
    }

    /// <summary>
    /// 更新订单项数量
    /// </summary>
    /// <param name="orderItemId">订单项ID</param>
    /// <param name="newQuantity">新数量</param>
    public void UpdateOrderItemQuantity(OrderItemId orderItemId, int newQuantity)
    {
        var orderItem = _orderItems.FirstOrDefault(x => x.Id == orderItemId);
        if (orderItem != null)
        {
            orderItem.UpdateQuantity(newQuantity);
            // 将子实体的领域事件添加到聚合根
            foreach (var domainEvent in orderItem.GetDomainEvents())
            {
                AddDomainEvent(domainEvent);
            }
            orderItem.ClearDomainEvents();
        }
    }

    /// <summary>
    /// 更新订单项单价
    /// </summary>
    /// <param name="orderItemId">订单项ID</param>
    /// <param name="newUnitPrice">新单价</param>
    public void UpdateOrderItemUnitPrice(OrderItemId orderItemId, decimal newUnitPrice)
    {
        var orderItem = _orderItems.FirstOrDefault(x => x.Id == orderItemId);
        if (orderItem != null)
        {
            orderItem.UpdateUnitPrice(newUnitPrice);
            // 将子实体的领域事件添加到聚合根
            foreach (var domainEvent in orderItem.GetDomainEvents())
            {
                AddDomainEvent(domainEvent);
            }
            orderItem.ClearDomainEvents();
        }
    }
}
