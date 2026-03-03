# 仓储

仓储提供了实体到数据库的存取能力，底层使用了`EntityFrameworkCore`作为具体的实现。

## 创建仓储

1. 添加nuget包`NetCorePal.Extensions.Repository.EntityFrameworkCore`：

    ```bash
    dotnet add package NetCorePal.Extensions.Repository.EntityFrameworkCore
    ```

2. 定义仓储

    ```csharp
    using NetCorePal.Extensions.Repository;
    using NetCorePal.Extensions.Repository.EntityFrameworkCore;

    namespace YourRepositoryNamespace;


    //仓储接口（可选），可以不定义仓储接口，仅定义仓储类。
    public interface IOrderRepository : IRepository<Order, OrderId>
    {
    }

    //仓储实现
    public class OrderRepository : RepositoryBase<Order, OrderId, ApplicationDbContext>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
    ```

3. 在Program.cs中注册仓储

    ```csharp
    using NetCorePal.Extensions.Repository.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    namespace YourStartupNamespace;

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
    });

    //注册仓储
    builder.Services.AddRepositories(typeof(OrderRepository).Assembly);

    //注册UnitOfWork
    builder.Services.AddUnitOfWork<ApplicationDbContext>();
    ```

    备注： 仓储类将被注册为`Scoped`生命周期。

## PostgreSQL（Npgsql）与 DateTimeOffset

使用 **PostgreSQL（Npgsql）** 时，数据库类型 `timestamp with time zone` 仅接受 **Offset=0（UTC）** 的 `DateTimeOffset`。若实体属性为 `DateTimeOffset`，而 API 反序列化得到的是带时区的值（例如 `2025-01-15T00:00:00+08:00`），保存时会抛出：

`ArgumentException: Cannot write DateTimeOffset with Offset=08:00:00 to PostgreSQL type 'timestamp with time zone', only offset 0 (UTC) is supported.`

**解决方式**：在注册 DbContext 时启用框架提供的可选补丁，在 `AddDbContext` 的 `DbContextOptionsBuilder` 上调用 `UseDateTimeOffsetUtcConversionForNpgsql()`，则所有 `DateTimeOffset`/`DateTimeOffset?` 属性在写入数据库前会自动转为 UTC。该选项**默认不启用**，仅在使用 Npgsql 且实体存在 `DateTimeOffset` 属性时按需开启。

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql"));
    options.UseDateTimeOffsetUtcConversionForNpgsql(); // 启用后，所有 DateTimeOffset 写入前转为 UTC
});
```
