# RowVersion

## 什么是RowVersion？

`RowVersion`是一种用于解决并发问题的机制，基于关系数据库特性的`乐观锁`的一种实现,在并发环境中，多个用户可能同时变更同一行数据，如果不加以限制，不同用户之间的操作可能相互覆盖，导致数据不一致的问题。`RowVersion`就是为了解决这个问题而设计的。

## RowVersion的实现原理

`RowVersion`的实现原理是在每一行数据中增加一个版本号字段，每次对这一行数据进行更新操作时，版本号加1。在事务开始时，事务会读取当前行的版本号，执行`update`时，会更新`RowVersion`，并在`where`条件中增加类似`row_version=@oldversion`这样的条件，事物提交前，会检查影响行数是否与预期一致，如果一致，则提交事务，否则回滚事务。

## RowVersion的使用场景

`RowVersion`主要用于解决并发问题，例如在订单系统中，当多个用户同时对同一订单进行操作时，可能会导致订单状态不一致的问题。通过使用行版本号，可以避免这种问题的发生。

## 定义行版本号

在领域模型中，定义一个`NetCorePal.Extensions.Domain.RowVersion`类型的`public`可读的属性，即可实现行版本号的功能，框架会自动处理行版本号的更新和并发检查逻辑。

下面是一个示例：

```csharp
// 定义行版本号
using NetCorePal.Extensions.Domain;
namespace YourNamespace;

//为模型定义强类型ID
public partial record OrderId : IInt64StronglyTypedId;

//领域模型
public class Order : Entity<OrderId>, IAggregateRoot
{
    protected Order() { }
    public string OrderNo { get; private set; } = string.Empty;
    public bool Paid { get; private set; }
    //定义行版本号
    public RowVersion Version { get; private set; } = new RowVersion();

    public void SetPaid()
    {
        Paid = true;
    }
}
```

## RowVersion不适合的场景

`RowVersion`适用于并发量较小的场景，如果并发量较大，可能会导致大量的行版本号冲突，从而影响用户体验。在这种情况下，可以考虑使用`悲观锁`来解决并发问题。