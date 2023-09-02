using NetCorePal.Extensions.Domain;

namespace NetCorePal.ConsoleApp
{
    public partial record OrderId222 : IInt64StronglyTypedId;

    public partial record OrderId2 : IInt64StronglyTypedId;

    public partial record Int32Id : IInt32StronglyTypedId;

    public partial record StrId : IStringStronglyTypedId;

    public partial record OrderZ : IGuidStronglyTypedId;
    
    
    
}
