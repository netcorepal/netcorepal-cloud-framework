using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses.DomainEvents;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

/// <summary>
/// 订单项ID强类型ID
/// </summary>
public partial record OrderItemId : IGuidStronglyTypedId;

/// <summary>
/// 订单项子实体（非聚合根）
/// </summary>
public class OrderItem : Entity<OrderItemId>
{
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    protected OrderItem()
    {
        ProductName = string.Empty;
    }

    public OrderItem(OrderItemId id, string productName, int quantity, decimal unitPrice)
    {
        Id = id;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        
        // 子实体发出领域事件
        AddDomainEvent(new OrderItemAddedDomainEvent(this));
    }

    /// <summary>
    /// 更新数量
    /// </summary>
    /// <param name="newQuantity">新数量</param>
    public void UpdateQuantity(int newQuantity)
    {
        var oldQuantity = Quantity;
        Quantity = newQuantity;
        
        // 子实体发出领域事件
        AddDomainEvent(new OrderItemQuantityChangedDomainEvent(this, oldQuantity, newQuantity));
    }

    /// <summary>
    /// 更新单价
    /// </summary>
    /// <param name="newUnitPrice">新单价</param>
    public void UpdateUnitPrice(decimal newUnitPrice)
    {
        var oldUnitPrice = UnitPrice;
        UnitPrice = newUnitPrice;
        
        // 子实体发出领域事件
        AddDomainEvent(new OrderItemPriceChangedDomainEvent(this, oldUnitPrice, newUnitPrice));
    }

    /// <summary>
    /// 移除订单项
    /// </summary>
    public void Remove()
    {
        // 子实体发出领域事件
        AddDomainEvent(new OrderItemRemovedDomainEvent(this));
    }
}
