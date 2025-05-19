# 按租户分库

按租户分库，是更具体的分库策略场景，在这个场景中大部分的业务处理都是限定在租户范围内的，因此可以保持在同一事务内提交。

## 配置分库


1. 添加包`NetCorePal.Extensions.ShardingCore`引用：

      ```shell
      dotnet add package NetCorePal.Extensions.ShardingCore
      ```
      或者 PackageReference
      ```
      <PackageReference Include="NetCorePal.Extensions.ShardingCore" />
      ```
2. 为你的 `DbContext` 类型添加 `IShardingCore` 接口

      ```csharp
      public partial class ApplicationDbContext : AppDbContextBase, IShardingCore
      {
          //Your Code
      }  
      ```
   
3. 创建`ApplicationDbContextCreator`

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

4. 移除 `AddDbContext` 注册方式
    ```chsarp
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(builder.Configuration.GetConnectionString("Mysql"),
                new MySqlServerVersion(new Version(8, 0, 34)),
                b => { b.MigrationsAssembly(typeof(Program).Assembly.FullName); });
        });
    
    ```

5. 添加包 `NetCorePal.Extensions.DistributedTransactions.CAP.MySql` 以支持CAP的发布消息分库：
    
    ```shell
    dotnet add package NetCorePal.Extensions.DistributedTransactions.CAP.MySql
    ```
    为 ApplicationDbContext 添加 IMySqlCapDataStorage 接口
     ```csharp
     public partial class ApplicationDbContext : AppDbContextBase, 
         IShardingCore, IMySqlCapDataStorage
     {
          //Your Code
     }
    ```

    修改AddCap代码，配置UseNetCorePalStorage
    ```csharp
    services.AddCap(op =>
    {
       op.UseNetCorePalStorage<ShardingDatabaseDbContext>(); //配置使用UseNetCorePalStorage 以支持分库
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

    MS SqlServer 以及 PostgreSql 也可以使用对应的包来支持
    ```shell
    dotnet add package NetCorePal.Extensions.DistributedTransactions.CAP.SqlServer
    dotnet add package NetCorePal.Extensions.DistributedTransactions.CAP.PostgreSql
    ```

6. 配置`MediatR`添加`AddTenantShardingBehavior`,注意需要添加在`AddUnitOfWorkBehaviors`之前:

     ```csharp
     services.AddMediatR(cfg =>
                      cfg.RegisterServicesFromAssembly(typeof(ShardingDatabaseDbContextTests).Assembly)
                          .AddTenantShardingBehavior()    //添加在`AddUnitOfWorkBehaviors`之前
                          .AddUnitOfWorkBehaviors());
   
     ```

7. 为分库的实体添加分库路由配置，分库需要实现基类`NetCorePalTenantVirtualDataSourceRoute`:

    ```csharp
    public class OrderTenantVirtualDataSourceRoute(
         IOptions<NetCorePalShardingCoreOptions> options,
         ITenantDataSourceProvider provider) : 
         NetCorePalTenantVirtualDataSourceRoute<Order, string>(options, provider)
    {
         public override void Configure(EntityMetadataDataSourceBuilder<Order> builder)
         {
              builder.ShardingProperty(p => p.TenantId); //返回租户Id
         }
    }
    ```

8. 配置ShardingCore:

    ```csharp
    services.AddShardingDbContext<ShardingDatabaseDbContext>()
                     .UseNetCorePal(op =>  //配置分库名称，需要UseConfig中配置的名称保持一致
                     {
                         op.AllDataSourceNames = ["Db0", "Db1"];
                         op.DefaultDataSourceName = "Db0";
                     })
                     .UseRouteConfig(op =>
                     {
                         op.AddCapShardingDataSourceRoute();  //添加默认的PubishedMessage分库路由
                         op.AddShardingDataSourceRoute<OrderTenantVirtualDataSourceRoute>();  //添加实体分库路由
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
   
9. 配置租户上下文支持，添加包`NetCorePal.Context.Shared`:
    ```shell
     dotnet add package NetCorePal.Context.Shared
    ```   
    注册租户上下文以及CAP上下文处理器：
    ```csharp
    services.AddTenantContext().AddCapContextProcessor();
    ```

10. 配置CAP上下文支持：
    ```csharp
    services.AddIntegrationEvents(typeof(ShardingTenantDbContext))
                     .UseCap<ShardingTenantDbContext>(capbuilder =>
                     {
                         capbuilder.AddContextIntegrationFilters(); //添加租户上下文过滤器
                         capbuilder.RegisterServicesFromAssemblies(typeof(ShardingTenantDbContext));
                     });
    ```
   
11. 实现`ITenantDataSourceProvider`并注册：

      ```csharp
      public class MyTenantDataSourceProvider : ITenantDataSourceProvider
      {
         public string GetDataSourceName(string tenantId)
         {
             return "Db" + (long.Parse(tenantId) % 10);  //实现租户Id与数据源名称的对应逻辑
         }
      }
      ```
      
      注册租户数据源提供程序：
      ```csharp
      services.AddSingleton<ITenantDataSourceProvider, MyTenantDataSourceProvider>();
      ```
   
## 使用租户上下文

在配置完成后，需要操作租户数据时，需要在 `Command` 发出前完成租户上下文的初始化，使用`IContextAccessor`设置当前上下文：

```csharp

//根据用户请求获取租户Id
var tenantId = currentUser.TenantId;
var contextAccessor = scope.ServiceProvider.GetRequiredService<IContextAccessor>();
contextAccessor.SetContext(new TenantContext(tenantId));
```

一般建议在中间件中完成租户上下文的设置：

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

## 高级

更多分库配置请参考官方文档：https://xuejmnet.github.io/sharding-core-doc/sharding-data-source/init/