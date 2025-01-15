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