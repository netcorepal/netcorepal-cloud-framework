# Repository

The repository provides the ability to access entities from the database, using `EntityFrameworkCore` as the underlying implementation.

## Creating a Repository

1. Add the NuGet package `NetCorePal.Extensions.Repository.EntityFrameworkCore`:

    ```bash
    dotnet add package NetCorePal.Extensions.Repository.EntityFrameworkCore
    ```

2. Define the repository

    ```csharp
    using NetCorePal.Extensions.Repository;
    using NetCorePal.Extensions.Repository.EntityFrameworkCore;

    namespace YourRepositoryNamespace;

    // Repository interface (optional), you can define only the repository class without the interface.
    public interface IOrderRepository : IRepository<Order, OrderId>
    {
    }

    // Repository implementation
    public class OrderRepository : RepositoryBase<Order, OrderId, ApplicationDbContext>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
    ```

3. Register the repository in Program.cs

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

    // Register the repository
    builder.Services.AddRepositories(typeof(OrderRepository).Assembly);

    // Register UnitOfWork
    builder.Services.AddUnitOfWork<ApplicationDbContext>();
    ```

    Note: Repository classes will be registered with a `Scoped` lifetime.

## PostgreSQL (Npgsql) and DateTimeOffset

When using **PostgreSQL (Npgsql)**, the database type `timestamp with time zone` only accepts `DateTimeOffset` with **Offset=0 (UTC)**. If an entity property is `DateTimeOffset` and the API deserializes a value with a non-UTC offset (e.g. `2025-01-15T00:00:00+08:00`), saving will throw:

`ArgumentException: Cannot write DateTimeOffset with Offset=08:00:00 to PostgreSQL type 'timestamp with time zone', only offset 0 (UTC) is supported.`

**Solution**: Enable the framework’s optional patch when registering the DbContext by calling `UseDateTimeOffsetUtcConversionForNpgsql()` on the `DbContextOptionsBuilder` in `AddDbContext`. All `DateTimeOffset`/`DateTimeOffset?` properties will then be converted to UTC before being written to the database. This option is **disabled by default**; enable it only when using Npgsql and your entities have `DateTimeOffset` properties.

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql"));
    options.UseDateTimeOffsetUtcConversionForNpgsql(); // When enabled, all DateTimeOffset values are converted to UTC before write
});
```
