# ID Generation

The framework currently provides two types of ID generators: `Guid` and `Int64`. You can configure the ID generator in `IEntityTypeConfiguration<>`. An example is shown below:

## Using the IGuidStronglyTypedId Generator

Supports two types of ID generators: `Guid` and `GuidVersion7`. It is recommended to use `GuidVersion7`.

```csharp

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
namespace YourNamespace;

public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Use Guid as ID
        builder.Property(x => x.Id).UseGuidValueGenerator();
        
        // Use Guid Version7 as ID
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
    }
}
```

## Using Snowflake ID as the IInt64StronglyTypedId Generator (Not Recommended)

Add the package `NetCorePal.Extensions.Repository.EntityFrameworkCore.Snowflake`:

```bash
dotnet add package NetCorePal.Extensions.Repository.EntityFrameworkCore.Snowflake;
```

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
namespace YourNamespace;

public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Use Int64 as ID
        builder.Property(x => x.Id).UseSnowFlakeValueGenerator();
    }
}
```
