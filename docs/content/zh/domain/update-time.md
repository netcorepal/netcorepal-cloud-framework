# 更新时间类型

## 介绍
`UpdateTime` 是一个值类型，内部封装了`DateTimeOffset`，用于表示一个时间戳，通常用于表示一个数据的更新时间。


## 如何使用

为实体定义一个`UpdateTime`类型的属性，即可，框架会自动处理其值的更新。

```csharp
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public class Order : Entity<OrderId>, IAggregateRoot
{
    /// <summary>
    /// 
    /// </summary>
    protected Order()
    {
    }

    /// <summary>
    /// 更新时间
    /// </summary>
    public UpdateTime UpdateAt { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);
}

```

## 控制数据库字段属性

使用`[Column]`特性来控制数据库字段的名称：

```csharp
using NetCorePal.Extensions.Domain;
using System.ComponentModel.DataAnnotations.Schema;
namespace YourNamespace;

public class Order : Entity<OrderId>, IAggregateRoot
{
    /// <summary>
    /// 
    /// </summary>
    protected Order()
    {
    }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Column(name:"update_at")]
    public UpdateTime UpdateAt { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);
}

```

## 比较操作

`UpdateTime` 类型支持比较操作符，可以直接比较两个 `UpdateTime` 值的大小关系。

### 支持的比较操作符

- `<` (小于)
- `>` (大于)
- `<=` (小于或等于)
- `>=` (大于或等于)

### 使用示例

```csharp
var time1 = new UpdateTime(DateTimeOffset.UtcNow);
await Task.Delay(100); // 等待一段时间
var time2 = new UpdateTime(DateTimeOffset.UtcNow);

// 比较两个更新时间
if (time1 < time2)
{
    Console.WriteLine("time1 早于 time2");
}

if (time2 > time1)
{
    Console.WriteLine("time2 晚于 time1");
}

// 可以用于排序或筛选
var orders = new List<Order>(); // 假设这是从数据库或其他来源获取的订单列表
var recentOrders = orders.Where(o => o.UpdateAt >= new UpdateTime(DateTimeOffset.UtcNow.AddDays(-7))).ToList();
```

### EF Core 使用注意事项

在 EF Core LINQ 查询中使用 `UpdateTime` 比较操作符时，**不能在表达式内部构造 `UpdateTime` 实例**。必须在查询表达式外部构造 `UpdateTime`。

**错误示例（在 EF Core 中不可行）：**
```csharp
// ❌ 这在 EF Core 中会失败
var recentOrders = dbContext.Orders
    .Where(o => o.UpdateAt >= new UpdateTime(DateTimeOffset.UtcNow.AddDays(-7)))
    .ToList();
```

**正确示例（在 EF Core 中可行）：**
```csharp
// ✅ 在表达式外部构造 UpdateTime
var sevenDaysAgo = new UpdateTime(DateTimeOffset.UtcNow.AddDays(-7));
var recentOrders = dbContext.Orders
    .Where(o => o.UpdateAt >= sevenDaysAgo)
    .ToList();
```