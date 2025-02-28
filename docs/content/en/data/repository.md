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
