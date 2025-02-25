# 软删除类型

## Deleted 类型

### 介绍

`Deleted` 是一个`record`类型，内部封装了`bool`值，默认为false，用于标记数据是否已被软删除。框架会根据此属性自动进行查询过滤。

### 如何使用

在实体类中，你可以定义 `Deleted` 类型的属性。需要注意的是，框架不会自动维护该属性的值，你需要自定义删除方法来更新此值。以下是一个示例：

```csharp
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

public class Order : Entity<OrderId>, IAggregateRoot
{
    protected Order() { }

    /// <summary>
    /// 删除标记
    /// </summary>
    public Deleted IsDeleted { get; private set; } = new ();

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}
```

在上述代码中，Order 实体类包含一个 IsDeleted 属性，初始值为 false，表示数据未被删除。SoftDelete 方法用于将 IsDeleted 属性设置为 true，标记数据已被软删除。

### 隐式转换

支持与`bool`类型的无缝转换：

```csharp
Deleted deleted = true;  // 自动转换为Deleted类型
bool isDeleted = deleted; // 自动解包为bool值
```

### 控制数据库字段属性

使用`[Column]`特性来控制数据库字段的名称：

```csharp
[Column(Name = "is_deleted")]
public Deleted IsDeleted { get; private set; }
```

## DeletedTime 类型

### 介绍

`DeletedTime` 是一个`record`类型，内部封装了`DateTimeOffset`，用于记录数据被软删除的时间。需与`Deleted`类型配合使用。

### 如何使用

在实体中定义`DeletedTime`类型的属性，框架会在实体中`Deleted`类型的属性值更新为true时自动设置时间戳：

```csharp
public class Order : Entity<OrderId>, IAggregateRoot
{
    protected Order() { }

    /// <summary>
    /// 删除标记
    /// </summary>
    public Deleted IsDeleted { get; private set; } = new Deleted(false);

    /// <summary>
    /// 删除时间
    /// </summary>
    public DeletedTime DeletedAt { get; private set; }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}
```

在上述代码中，Order 实体类包含 IsDeleted 和 DeletedAt 两个属性。当调用 SoftDelete 方法将 IsDeleted 设置为 true 时，框架会自动为 DeletedAt 属性记录当前时间。

### 控制数据库字段属性

使用`[Column]`特性来控制数据库字段的名称：

```csharp
[Column(Name = "deleted_at")]
public DeletedTime DeletedAt { get; private set; }
```

## 最佳实践

为了更方便地在多个实体类中使用软删除功能，建议自定义一个接口，将 Deleted 和 DeletedTime 类型的属性组合在一起。示例如下：

```csharp
public interface ISoftDelete
{
    public Deleted IsDeleted { get; private set; }
    public DeletedTime DeletedAt { get; private set; }
}
```