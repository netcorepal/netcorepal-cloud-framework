# UpdateTime Type

## Introduction
`UpdateTime` is a value type that encapsulates `DateTimeOffset` internally. It is used to represent a timestamp, usually indicating the last update time of a data record.

## How to Use

Define a property of type `UpdateTime` in your entity, and the framework will automatically handle its value updates.

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
    /// Update time
    /// </summary>
    public UpdateTime UpdateAt { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);
}

```

## Control Database Field Properties

Use the `[Column]` attribute to control the database field name:

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
    /// Update time
    /// </summary>
    [Column(name:"update_at")]
    public UpdateTime UpdateAt { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);
}

```

## Comparison Operations

The `UpdateTime` type supports comparison operators, allowing you to directly compare the temporal relationship between two `UpdateTime` values.

### Supported Comparison Operators

- `<` (less than)
- `>` (greater than)
- `<=` (less than or equal to)
- `>=` (greater than or equal to)

### Usage Examples

```csharp
var time1 = new UpdateTime(DateTimeOffset.UtcNow);
await Task.Delay(100); // Wait for some time
var time2 = new UpdateTime(DateTimeOffset.UtcNow);

// Compare two update times
if (time1 < time2)
{
    Console.WriteLine("time1 is earlier than time2");
}

if (time2 > time1)
{
    Console.WriteLine("time2 is later than time1");
}

// Can be used for sorting or filtering
var orders = new List<Order>(); // Assume this is populated from database or other source
var recentOrders = orders.Where(o => o.UpdateAt >= new UpdateTime(DateTimeOffset.UtcNow.AddDays(-7))).ToList();
```

### Important Notes for EF Core

When using `UpdateTime` comparison operators in EF Core LINQ queries, you **cannot construct `UpdateTime` instances inside the expression**. The `UpdateTime` construction must be done outside the query expression.

**Incorrect (will not work with EF Core):**
```csharp
// ❌ This will fail with EF Core
var recentOrders = dbContext.Orders
    .Where(o => o.UpdateAt >= new UpdateTime(DateTimeOffset.UtcNow.AddDays(-7)))
    .ToList();
```

**Correct (works with EF Core):**
```csharp
// ✅ Construct UpdateTime outside the expression
var sevenDaysAgo = new UpdateTime(DateTimeOffset.UtcNow.AddDays(-7));
var recentOrders = dbContext.Orders
    .Where(o => o.UpdateAt >= sevenDaysAgo)
    .ToList();
```