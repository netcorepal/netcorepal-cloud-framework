# Read-Write Separation

As the number of system users increases, the query pressure on the system grows. Using a read-write separation solution can effectively alleviate database query pressure.

## Prerequisites

`sharding-core` does not implement data synchronization to read-only databases. To adopt a read-write separation solution, you need to establish a database data synchronization mechanism, such as `MySQL`'s `binlog` synchronization or `MSSQL`'s `Always On` feature.

## Configuring Read-Write Separation

To use `sharding-core`, you need to modify the registration method of `DbContext`.

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

5. Use `AddShardingDbContext` for registration. The following configuration sets up two read-only databases for the data source named `ds0`:

    ```csharp
    builder.Services.AddShardingDbContext<ApplicationDbContext>()
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
                op.AddReadWriteSeparation(sp => new Dictionary<string, IEnumerable<string>>    // Configure read-only databases
                    {
                        {
                            "ds0",
                            [builder.Configuration.GetConnectionString("Mysql-Read1"), builder.Configuration.GetConnectionString("Mysql-Read2")]
                        }
                    },
                    readStrategyEnum: ReadStrategyEnum.Loop,
                    defaultEnableBehavior: ReadWriteDefaultEnableBehavior.DefaultDisable,
                    defaultPriority: 10,
                    readConnStringGetStrategy: ReadConnStringGetStrategyEnum.LatestFirstTime
                );
            })
            .ReplaceService<IDbContextCreator, ApplicationDbContextCreator>()
            .AddShardingCore();
    ```

## Reference Documentation

For more configurations and usage, refer to:

[https://xuejmnet.github.io/sharding-core-doc/read-write/configure](https://xuejmnet.github.io/sharding-core-doc/read-write/configure)