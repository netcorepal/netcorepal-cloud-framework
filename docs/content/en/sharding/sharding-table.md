# Table Sharding

When the data volume of a business table becomes too large and affects query performance, table sharding can be used to split the data. `sharding-core` supports sharding by `time`, `modulus`, and more.

## Configuring Table Sharding

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

4. Add table sharding configuration:

    ```csharp
    public class OrderVirtualTableRoute : AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<Order>
    {
        public override DateTime GetBeginTime()
        {
            return DateTime.Now.AddMonths(-3);
        }
   
        public override void Configure(EntityMetadataTableBuilder<Order> builder)
        {
            builder.ShardingProperty(o => o.CreationTime);
        }
   
        public override bool AutoCreateTableByTime()
        {
            return true;
        }
    }
    ```

5. Use `AddShardingDbContext` to register `ApplicationDbContext`:

    ```csharp
    builder.Services.AddShardingDbContext<ApplicationDbContext>()
            .UseRouteConfig(op =>
            {
               op.AddShardingTableRoute<OrderVirtualTableRoute>(); // Register table sharding configuration
            })
            .UseConfig(op =>
            {
                op.ThrowIfQueryRouteNotMatch = true;
                op.UseShardingQuery((conStr, builder) =>
                {
                    builder.UseMySql(conStr,
                        new MySqlServerVersion(new Version(8, 0, 34)));
                });
                op.UseShardingTransaction((con, builder) =>
                {
                    builder.UseMySql(con,
                        new MySqlServerVersion(new Version(8, 0, 34)));
                });
                op.AddDefaultDataSource("ds0", builder.Configuration.GetConnectionString("Mysql")); // Configure write database
            })
            .ReplaceService<IDbContextCreator, ApplicationDbContextCreator>()
            .AddShardingCore();
    ```

## Advanced

For more advanced table sharding configurations, refer to the official documentation: https://xuejmnet.github.io/sharding-core-doc/sharding-table/init/

