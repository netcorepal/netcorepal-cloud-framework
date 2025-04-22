# ID 生成

目前框架提供了两种ID生成器，分别是`Guid`和`Int64`，可以在`IEntityTypeConfiguration<>`中配置ID生成器，示例如下：


## 使用 IGuidStronglyTypedId 生成器

支持`Guid`和`GuidVersion7`两种类型的ID生成器，推荐使用`GuidVersion7`。

```csharp

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCorePal.Extensions.Repository.EntityFrameworkCore;
namespace YourNamespace;

public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        //使用Guid作为ID
        builder.Property(x => x.Id).UseGuidValueGenerator();
        
        //使用Guid Version7 作为ID
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
    }
}
```

## 使用雪花ID作为 IInt64StronglyTypedId 生成器 (不推荐)

添加包`NetCorePal.Extensions.Repository.EntityFrameworkCore.Snowflake`:

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
        //使用Int64作为ID
        builder.Property(x => x.Id).UseSnowFlakeValueGenerator();
    }
}
```
