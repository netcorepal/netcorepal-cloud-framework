# 强类型实体ID

```csharp
using NetCorePal.Extensions.Domain;
namespace YourNamespace
{
    public partial record OrderId : IInt64StronglyTypedId;
}
```

```csharp
using NetCorePal.Extensions.Domain;
using System;
using System.ComponentModel;
namespace YourNamespace
{
    [TypeConverter(typeof(EntityIdTypeConverter<OrderId, Int64>))]
    public partial record OrderId(Int64 Id) : IInt64StronglyTypedId
    {
        public static implicit operator Int64(OrderId222 id) => id.Id;
        public static implicit operator OrderId(Int64 id) => new OrderId(id);
        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
```