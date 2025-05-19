# 分表

当某个业务表数据量规模比较大而影响查询性能时，可以采取分表的方式将数据拆分开，`sharding-core` 支持按照`时间`、`取模`等

## 配置分表

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
5. 为表添加分表配置：

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

6. 使用 `AddShardingDbContext` 注册`ApplicationDbContext`:

    ```csharp
    builder.Services.AddShardingDbContext<ApplicationDbContext>()
            .UseRouteConfig(op =>
            {
               op.AddShardingTableRoute<OrderVirtualTableRoute>(); //注册分表配置
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
                op.AddDefaultDataSource("ds0", builder.Configuration.GetConnectionString("Mysql")); //配置写库
            })
            .ReplaceService<IDbContextCreator, ApplicationDbContextCreator>()
            .AddShardingCore();
    ```


## 高级

更多关于分表的配置和使用可以参考官方文档： https://xuejmnet.github.io/sharding-core-doc/sharding-table/init/

