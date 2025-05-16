# 分库

当我们发现单个数据库的性能无法满足需求时，我们可以考虑将数据分散到多个数据库中，


## 关于事务

由于分库会产生跨数据库事务的问题，尤其是集成事件的本地消息需要共享当前上下文的事务，因此我们实现了在同一个命令执行范围内，确保集成事件的发布记录保存到与业务数据相同的数据源中。

因此会有如下注意事项：

1. 同一个命令中只操作同一分库的数据时，能够确保集成事件的发布记录保存到与业务数据相同的数据源中。
2. 同一个请求上下文涉及不同分库时，`sharding-core` 会为每个对应数据源开启各自的事务。

更多关于分库事务的说明，请参考：https://xuejmnet.github.io/sharding-core-doc/adv/transaction/

## 配置分库

1. 添加包`NetCorePal.Extensions.ShardingCore`引用：

      ```shell
      dotnet add package NetCorePal.Extensions.ShardingCore
      ```
      或者 PackageReference
      ```
      <PackageReference Include="NetCorePal.Extensions.ShardingCore" />
      ```

2. 创建`ApplicationDbContextCreator`

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

3. 移除 `AddDbContext` 注册方式
    ```chsarp
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(builder.Configuration.GetConnectionString("Mysql"),
                new MySqlServerVersion(new Version(8, 0, 34)),
                b => { b.MigrationsAssembly(typeof(Program).Assembly.FullName); });
        });
    
    ```
   
4. 添加包 `NetCorePal.Extensions.DistributedTransactions.CAP.MySql` 以支持CAP的发布消息分库：
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
   
5. 配置`MediatR`添加`AddShardingBehavior`,注意需要添加在`AddUnitOfWorkBehaviors`之前:

      ```csharp
      services.AddMediatR(cfg =>
                       cfg.RegisterServicesFromAssembly(typeof(ShardingDatabaseDbContextTests).Assembly)
                           .AddShardingBehavior()    //添加在`AddUnitOfWorkBehaviors`之前
                           .AddUnitOfWorkBehaviors());
   
      ```
   
   6. 为分库的实体添加分库路由配置，分库需要实现基类`NetCorePalVirtualDataSourceRoute`:

      ```csharp
      public class OrderVirtualDataSourceRoute(IOptions<NetCorePalShardingCoreOptions> options)
       : NetCorePalVirtualDataSourceRoute<Order,
           string>(options)
      {
          public override void Configure(EntityMetadataDataSourceBuilder<Order> builder)
          {
              builder.ShardingProperty(o => o.Area); //返回分库字段
          }
   
          protected override string GetDataSourceName(object? shardingKey)
          {
              return shardingKey == null ? string.Empty : shardingKey.ToString()!; //实现自定义分库逻辑
          }
      }
      ```

   7. 配置ShardingCore:

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
                           op.AddShardingDataSourceRoute<OrderVirtualDataSourceRoute>();  //添加实体分库路由
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
   

## 高级

更多分库配置请参考官方文档：https://xuejmnet.github.io/sharding-core-doc/sharding-data-source/init/