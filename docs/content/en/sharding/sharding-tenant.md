# Tenant-Based Database Sharding

Tenant-based database sharding is a more specific database sharding strategy. In this scenario, most business operations are confined to the tenant's scope, allowing transactions to be committed within the same transaction.

## Configuring Database Sharding

1. Add the `NetCorePal.Extensions.ShardingCore` package:

      ```shell
      dotnet add package NetCorePal.Extensions.ShardingCore
      ```
      Or use PackageReference:
      ```
      <PackageReference Include="NetCorePal.Extensions.ShardingCore" />
      ```

2. Add the `IShardingCore` interface to your `DbContext` type:

      ```csharp
      public partial class ApplicationDbContext : AppDbContextBase, IShardingCore
      {
          // Your Code
      }
      ```

3. Create `ApplicationDbContextCreator`:

    ```csharp
    public class ApplicationDbContextCreator(IShardingProvider provider)
        : IDbContextCreator
    {
        public DbContext CreateDbContext(DbContext shellDbContext, ShardingDbContextOptions shardingDbContextOptions)
        {
            var outDbContext = (ApplicationDbContext)shellDbContext;
            var dbContext = new ApplicationDbContext(
                (DbContextOptions<ApplicationDbContext>)shardingDbContextOptions.DbContextOptions, outDbContext.Mediator);
            if (dbContext is IShardingTableDbContext shardingTableDbContext)
            {
                shardingTableDbContext.RouteTail = shardingDbContextOptions.RouteTail;
            }
    
            _ = dbContext.Model;
            return dbContext;
        }
    
        public DbContext GetShellDbContext(IShardingProvider shardingProvider)
        {
            return shardingProvider.GetRequiredService<ApplicationDbContext>();
        }
    }
    ```

4. Remove the `AddDbContext` registration method:
    ```csharp
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(builder.Configuration.GetConnectionString("Mysql"),
                new MySqlServerVersion(new Version(8, 0, 34)),
                b => { b.MigrationsAssembly(typeof(Program).Assembly.FullName); });
        });
    ```

5. Add the `NetCorePal.Extensions.DistributedTransactions.CAP.MySql` package to support CAP's message publishing for sharded databases:
    ```shell
    dotnet add package NetCorePal.Extensions.DistributedTransactions.CAP.MySql
    ```
    Add the `IMySqlCapDataStorage` interface to `ApplicationDbContext`:
    ```csharp
    public partial class ApplicationDbContext : AppDbContextBase, 
      IShardingCore, IMySqlCapDataStorage
    {
       // Your Code
    }
    ```

    Modify the `AddCap` code to configure `UseNetCorePalStorage`:
    ```csharp
    services.AddCap(op =>
    {
       op.UseNetCorePalStorage<ShardingDatabaseDbContext>(); // Configure UseNetCorePalStorage to support sharding
       op.UseRabbitMQ(p =>
       {
          p.HostName = _rabbitMqContainer.Hostname;
          p.UserName = "guest";
          p.Password = "guest";
          p.Port = _rabbitMqContainer.GetMappedPublicPort(5672);
          p.VirtualHost = "/";
       });
    });
    ```

    MS SqlServer and PostgreSql can also use corresponding packages for support:
    ```shell
    dotnet add package NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer
    dotnet add package NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql
    ```

6. Configure `MediatR` to add `AddTenantShardingBehavior`. Note that it must be added before `AddUnitOfWorkBehaviors`:

    ```csharp
    services.AddMediatR(cfg =>
                     cfg.RegisterServicesFromAssembly(typeof(ShardingDatabaseDbContextTests).Assembly)
                         .AddTenantShardingBehavior()    // Add before `AddUnitOfWorkBehaviors`
                         .AddUnitOfWorkBehaviors());
    ```

7. Add sharding route configuration for sharded entities. Sharding requires implementing the base class `NetCorePalTenantVirtualDataSourceRoute`:

    ```csharp
    public class OrderTenantVirtualDataSourceRoute(
         IOptions<NetCorePalShardingCoreOptions> options,
         ITenantDataSourceProvider provider) : 
         NetCorePalTenantVirtualDataSourceRoute<Order, string>(options, provider)
    {
         public override void Configure(EntityMetadataDataSourceBuilder<Order> builder)
         {
              builder.ShardingProperty(p => p.TenantId); // Return TenantId
         }
    }
    ```

8. Configure ShardingCore:

    ```csharp
    services.AddShardingDbContext<ShardingDatabaseDbContext>()
                     .UseNetCorePal(op =>  // Configure sharding names consistent with UseConfig
                     {
                         op.AllDataSourceNames = ["Db0", "Db1"];
                         op.DefaultDataSourceName = "Db0";
                     })
                     .UseRouteConfig(op =>
                     {
                         op.AddCapShardingDataSourceRoute();  // Add default PublishedMessage sharding route
                         op.AddShardingDataSourceRoute<OrderTenantVirtualDataSourceRoute>();  // Add entity sharding route
                     }).UseConfig(op =>
                     {
                         op.ThrowIfQueryRouteNotMatch = true;
                         op.UseShardingQuery((conStr, builder) =>
                         {
                             builder.UseMySql(conStr,
                                 new MySqlServerVersion(new Version(8, 0, 34)));
                         });
                         op.UseShardingTransaction((conStr, builder) =>
                         {
                             builder.UseMySql(conStr,
                                 new MySqlServerVersion(new Version(8, 0, 34)));
                         });
                         op.AddDefaultDataSource("Db0", _mySqlContainer0.GetConnectionString());
                         op.AddExtraDataSource(_ => new Dictionary<string, string>
                         {
                             { "Db1", _mySqlContainer1.GetConnectionString() }
                         });
                     })
                     .ReplaceService<IDbContextCreator, ShardingDatabaseDbContextCreator>()
                     .AddShardingCore();
    ```

9. Configure tenant context support by adding the `NetCorePal.Context.Shared` package:
   
    ```shell
     dotnet add package NetCorePal.Context.Shared
    ```   
    Register tenant context and CAP context processors:
    ```csharp
    services.AddTenantContext().AddCapContextProcessor();
    ```

10. Configure CAP context support:

    ```csharp
    services.AddIntegrationEvents(typeof(ShardingTenantDbContext))
                     .UseCap<ShardingTenantDbContext>(capbuilder =>
                     {
                         capbuilder.AddContextIntegrationFilters(); // Add tenant context filters
                         capbuilder.RegisterServicesFromAssemblies(typeof(ShardingTenantDbContext));
                     });
    ```

11. Implement `ITenantDataSourceProvider` and register it:

      ```csharp
      public class MyTenantDataSourceProvider : ITenantDataSourceProvider
      {
         public string GetDataSourceName(string tenantId)
         {
             return "Db" + (long.Parse(tenantId) % 10);  // Implement tenantId to data source name mapping logic
         }
      }
      ```
   
      Register the tenant data source provider:
      ```csharp
      services.AddSingleton<ITenantDataSourceProvider, MyTenantDataSourceProvider>();
      ```

## Using Tenant Context

After configuration, when operating tenant data, initialize the tenant context before issuing a `Command` using `IContextAccessor`:

```csharp
// Get tenantId from user request
var tenantId = currentUser.TenantId;
var contextAccessor = scope.ServiceProvider.GetRequiredService<IContextAccessor>();
contextAccessor.SetContext(new TenantContext(tenantId));
```

It is generally recommended to set the tenant context in middleware:

```csharp
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IContextAccessor contextAccessor)
    {
        var tenantId = context.Request.Headers["TenantId"].ToString();
        if (!string.IsNullOrEmpty(tenantId))
        {
            contextAccessor.SetContext(new TenantContext(tenantId));
        }
        await _next(context);
    }
}
```

## Advanced

For more database sharding configurations, refer to the official documentation: https://xuejmnet.github.io/sharding-core-doc/sharding-data-source/init/