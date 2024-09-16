using NetCorePal.Extensions.Domain;

namespace NetCorePal.Web.Domain;

public partial record OrderItemId : IInt64StronglyTypedId;

/// <summary>
/// 
/// </summary>
public class OrderItem : Entity<OrderItemId>
{
    /// <summary>
    /// 
    /// </summary>
    protected OrderItem()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="count"></param>
    public OrderItem(string name, int count)
    {
        this.Name = name;
        this.Count = count;
    }

    /// <summary>
    /// 
    /// </summary>

    public OrderId OrderId { get; private set; } = default!;
    /// <summary>
    /// 
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public int Count { get; private set; }
    /// <summary>
    /// 
    /// </summary>
    public RowVersion RowVersion { get; private set; } = new RowVersion();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newName"></param>
    public void ChangeName(string newName)
    {
        this.Name = newName;
    }
}