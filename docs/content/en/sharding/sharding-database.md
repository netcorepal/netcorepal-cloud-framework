# Database Sharding

When the performance of a single database cannot meet the requirements, we can consider distributing the data across multiple databases.

## About Transactions

Database sharding introduces cross-database transaction issues. Especially for local messages of integration events, it is necessary to ensure that the publication records of integration events are saved in the same data source as the business data within the same command execution scope.

Key considerations include:

1. When operating on data from the same shard within the same command, the publication records of integration events can be saved in the same data source as the business data.
2. When a single request context involves different shards, `sharding-core` will open separate transactions for each corresponding data source.

For more details on database sharding transactions, refer to: https://xuejmnet.github.io/sharding-core-doc/adv/transaction/

## Configuring Database Sharding

1. Add the `NetCorePal.Extensions.ShardingCore` package:

      ```shell
      dotnet add package NetCorePal.Extensions.ShardingCore
      ```
      Or use PackageReference:
      ```
      <PackageReference Include="NetCorePal.Extensions.ShardingCore" />
      ```

2. Create `ApplicationDbContextCreator`:

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

3. Remove the `AddDbContext` registration method:
    ```csharp
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(builder.Configuration.GetConnectionString("Mysql"),
                new MySqlServerVersion(new Version(8, 0, 34)),
                b => { b.MigrationsAssembly(typeof(Program).Assembly.FullName); });
        });
    ```

4. Add the `NetCorePal.Extensions.DistributedTransactions.CAP.MySql` package to support CAP message publishing across shards:
    ```shell
    dotnet add package NetCorePal.Extensions.DistributedTransactions.CAP.MySql
    ```
    Implement the `IMySqlCapDataStorage` interface in `ApplicationDbContext`:
    ```csharp
    public partial class ApplicationDbContext : AppDbContextBase, 
        IShardingCore, IMySqlCapDataStorage
    {
         // Your code
    }
    ```

    Modify `AddCap` configuration to use `UseNetCorePalStorage`:
    ```csharp
    services.AddCap(op =>
    {
        op.UseNetCorePalStorage<ShardingDatabaseDbContext>();
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

5. Configure `MediatR` to add `AddShardingBehavior` before `AddUnitOfWorkBehaviors`:

    ```csharp
    services.AddMediatR(cfg =>
                     cfg.RegisterServicesFromAssembly(typeof(ShardingDatabaseDbContextTests).Assembly)
                         .AddShardingBehavior()
                         .AddUnitOfWorkBehaviors());
    ```

6. Add shard routing configuration for entities. Database sharding requires implementing the `NetCorePalVirtualDataSourceRoute` base class:

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

7. Configure `ShardingCore`:

    ```csharp
    services.AddShardingDbContext<ShardingDatabaseDbContext>()
                     .UseNetCorePal(op =>
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

## Advanced

For more advanced database sharding configurations, refer to the official documentation: https://xuejmnet.github.io/sharding-core-doc/sharding-data-source/init/