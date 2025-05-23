using NetCorePal.Extensions.Domain;

namespace NetCorePal.Web.Domain;


public partial record GuidOrderId : IGuidStronglyTypedId;

public class GuidOrder : Entity<GuidOrderId>
{
    protected GuidOrder()
    {
    }

    public GuidOrder(string name)
    {
        this.Name = name;
    }

    public string Name { get; private set; } = string.Empty;
}