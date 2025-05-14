# 读写分离

随着系统用户的增多，系统的查询压力会越来越大，此时使用读写分离方案，可以有效缓解数据库的查询压力。

## 前提条件

`sharding-core` 未实现数据同步到只读库的功能，要采用读写分离的方案，需要先做好数据库的数据同步机制，例如 `MySQL` 的 `binlog` 同步、`MSSQL` 的 `always on`功能等。

## 配置读写分离

要使用`sharding-core`,需要修改DbContext的注册方式。

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

4. 使用 `AddShardingDbContext` 注册,下面配置为名称为`ds0`的数据源配置了两个只读库：

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
                op.AddDefaultDataSource("ds0", builder.Configuration.GetConnectionString("Mysql")); //配置写库
                op.AddReadWriteSeparation(sp => new Dictionary<string, IEnumerable<string>>    //配置只读库
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

## 参考文档

更多的配置和使用可以参考：

[https://xuejmnet.github.io/sharding-core-doc/read-write/configure](https://xuejmnet.github.io/sharding-core-doc/read-write/configure)