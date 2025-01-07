# 命令锁
顾名思义，命令锁是为了解决命令并发执行的问题。在一些场景下，我们需要保证某个命令在同一时间只能被一个实例执行，这时候就可以使用命令锁。
本质上命令锁是一种分布式锁，它的实现方式有很多种，我们默认提供了基于Redis来实现的命令锁。

## 注册命令锁

在Program.cs注册`CommandLocks`：
```csharp
builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly())
            .AddCommandLockBehavior()  //注册命令锁行为
            .AddUnitOfWorkBehaviors()
            .AddKnownExceptionValidationBehavior());

builder.Services.AddCommandLocks(typeof(Program).Assembly); //注册所有的命令锁类型
```

注意： 命令锁应该在事务开启前执行，所以需要在`AddUnitOfWorkBehaviors`之前添加`AddCommandLockBehavior`。


## 使用命令锁

定义一个命令锁，实现`ICommandLock<TCommand>`接口，其中`TCommand`是命令类型，例如：

```csharp
public record PayOrderCommand(OrderId Id) : ICommand<OrderId>;

public class PayOrderCommandLock : ICommandLock<PayOrderCommand>
{
    public Task<CommandLockSettings> GetLockKeysAsync(PayOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(command.Id.ToCommandLockSettings());
    }
}
```

其中`command.Id.ToCommandLockSettings()`是将`OrderId`转换为`CommandLockSettings`，`CommandLockSettings`是命令锁的配置，包含了锁的Key、获取锁之前可以等待的过期时间。

设计上，命令锁与命令是一对一关系，建议将命令锁与命令、命令处理器放在同一个类文件中，便于维护。

## 多key命令锁

命令锁支持多Key机制，即一个命令可以对应多个Key，例如：

```csharp

public class PayOrderCommandLock : ICommandLock<PayOrderCommand>
{
    public Task<CommandLockSettings> GetLockKeysAsync(PayOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var ids = new List<OrderId> { new OrderId(1), new OrderId(2) };
        return Task.FromResult(ids.ToCommandLockSettings());
    }
}
```

在这个例子中，`PayOrderCommand`对应两个Key，分别是`OrderId(1)`和`OrderId(2)`。

当需要锁定多个Key时，CommandLockSettings会对多个Key进行排序，然后逐个锁定，如果其中一个Key锁定失败，则会释放已经锁定的Key。


## 可重入机制

命令锁实现了可重入机制，即在同一个请求上下文中，相同的Key可以重复获取锁，不会造成死锁。
例如上面示例的命令执行后序的事件处理过程中再次执行携带相同Key的命令锁，不会死锁。