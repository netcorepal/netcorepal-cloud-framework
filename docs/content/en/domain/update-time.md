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