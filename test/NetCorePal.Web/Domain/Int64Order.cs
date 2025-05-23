using NetCorePal.Extensions.Domain;

namespace NetCorePal.Web.Domain;

public partial record Int64OrderId : IInt64StronglyTypedId;

public class Int64Order : Entity<Int64OrderId>
{
    protected Int64Order()
    {
    }

    public Int64Order(string name)
    {
        this.Name = name;
    }

    public string Name { get; private set; } = string.Empty;
}