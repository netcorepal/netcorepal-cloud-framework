using NetCorePal.Extensions.Domain;

namespace NetCorePal.Web.Domain;

public partial record Int32OrderId : IInt32StronglyTypedId;

public class Int32Order : Entity<Int32OrderId>
{
    protected Int32Order()
    {
    }

    public Int32Order(string name)
    {
        this.Name = name;
    }

    public string Name { get; private set; } = string.Empty;
}