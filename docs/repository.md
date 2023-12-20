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
