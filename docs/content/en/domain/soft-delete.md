# Soft Delete Type

## Deleted Type

### Introduction

`Deleted` is a `record` type that encapsulates a `bool` value internally, defaulting to false. It is used to mark whether data has been soft deleted. The framework will automatically filter queries based on this property.

### How to Use

In the entity class, you can define a property of type `Deleted`. Note that the framework will not automatically maintain the value of this property; you need to customize the delete method to update this value. Here is an example:

```csharp
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public class Order : Entity<OrderId>, IAggregateRoot
{
    protected Order() { }

    /// <summary>
    /// Delete flag
    /// </summary>
    public Deleted IsDeleted { get; private set; } = new ();

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}
```

In the above code, the Order entity class contains an IsDeleted property, which is initially set to false, indicating that the data has not been deleted. The SoftDelete method is used to set the IsDeleted property to true, marking the data as soft deleted.

### Implicit Conversion

Supports seamless conversion with the `bool` type:

```csharp
Deleted deleted = true;  // Automatically converts to Deleted type
bool isDeleted = deleted; // Automatically unpacks to bool value
```

### Control Database Field Properties

Use the `[Column]` attribute to control the database field name:

```csharp
[Column(Name = "is_deleted")]
public Deleted IsDeleted { get; private set; }
```

## DeletedTime Type

### Introduction

`DeletedTime` is a `record` type that encapsulates `DateTimeOffset` internally. It is used to record the time when the data was soft deleted. It needs to be used in conjunction with the `Deleted` type.

### How to Use

Define a property of type `DeletedTime` in the entity. The framework will automatically set the timestamp when the value of the `Deleted` type property in the entity is updated to true:

```csharp
public class Order : Entity<OrderId>, IAggregateRoot
{
    protected Order() { }

    /// <summary>
    /// Delete flag
    /// </summary>
    public Deleted IsDeleted { get; private set; } = new Deleted(false);

    /// <summary>
    /// Delete time
    /// </summary>
    public DeletedTime DeletedAt { get; private set; }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}
```

In the above code, the Order entity class contains two properties, IsDeleted and DeletedAt. When the SoftDelete method is called to set IsDeleted to true, the framework will automatically record the current time for the DeletedAt property.

### Control Database Field Properties

Use the `[Column]` attribute to control the database field name:

```csharp
[Column(Name = "deleted_at")]
public DeletedTime DeletedAt { get; private set; }
```

## Best Practices

To make it easier to use the soft delete feature in multiple entity classes, it is recommended to customize an interface that combines the Deleted and DeletedTime type properties. Here is an example:

```csharp
public interface ISoftDelete
{
    public Deleted IsDeleted { get; private set; }
    public DeletedTime DeletedAt { get; private set; }
}
```