using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests.TestClasses;

/// <summary>
/// 订单项ID强类型ID
/// </summary>
public partial record OrderItemId : IGuidStronglyTypedId;

/// <summary>
/// 订单项实体
/// </summary>
public class OrderItem : Entity<OrderItemId>
{
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

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
    }

    /// <summary>
    /// 更新数量
    /// </summary>
    /// <param name="newQuantity">新数量</param>
    public void UpdateQuantity(int newQuantity)
    {
        Quantity = newQuantity;
    }

    /// <summary>
    /// 更新单价
    /// </summary>
    /// <param name="newUnitPrice">新单价</param>
    public void UpdateUnitPrice(decimal newUnitPrice)
    {
        UnitPrice = newUnitPrice;
    }
}
