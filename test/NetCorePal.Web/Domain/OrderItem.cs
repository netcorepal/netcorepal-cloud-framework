using NetCorePal.Extensions.Domain;

namespace NetCorePal.Web.Domain;

public partial record OrderItemId : IInt64StronglyTypedId;

public class OrderItem : Entity<OrderItemId>
{
    protected OrderItem()
    {
    }

    public OrderItem(string name, int count)
    {
        this.Name = name;
        this.Count = count;
    }


    public OrderId OrderId { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public int Count { get; private set; }

    public RowVersion RowVersion { get; private set; } = new RowVersion();

    public void ChangeName(string newName)
    {
        this.Name = newName;
    }
}