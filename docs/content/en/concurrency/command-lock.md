# Command Lock

As the name suggests, command lock is used to solve the problem of concurrent execution of commands. In some scenarios, we need to ensure that a command can only be executed by one instance at the same time. At this time, we can use command lock.
Essentially, command lock is a kind of distributed lock, and there are many ways to implement it. By default, we provide a command lock implemented based on Redis.

## Register Command Lock

Register `CommandLocks` in `Program.cs`:
```csharp
builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly())
            .AddCommandLockBehavior()  // Register command lock behavior
            .AddKnownExceptionValidationBehavior()
            .AddUnitOfWorkBehaviors());

builder.Services.AddCommandLocks(typeof(Program).Assembly); // Register all command lock types
```

Note: Command lock should be executed before the transaction starts, so it needs to be added before `AddKnownExceptionValidationBehavior`.

## Use Command Lock

Define a command lock and implement the `ICommandLock<TCommand>` interface, where `TCommand` is the command type, for example:

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

Where `command.Id.ToCommandLockSettings()` converts `OrderId` to `CommandLockSettings`. `CommandLockSettings` is the configuration of the command lock, including the key of the lock and the expiration time that can be waited before acquiring the lock.

In design, command lock and command have a one-to-one relationship. It is recommended to place the command lock, command, and command handler in the same class file for easy maintenance.

## Multi-key Command Lock

Command lock supports the multi-key mechanism, that is, a command can correspond to multiple keys, for example:

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

In this example, `PayOrderCommand` corresponds to two keys, `OrderId(1)` and `OrderId(2)`.

When multiple keys need to be locked, CommandLockSettings will sort the multiple keys and then lock them one by one. If one of the keys fails to lock, the already locked keys will be released.

## Reentrant Mechanism

Command lock implements a reentrant mechanism, that is, in the same request context, the same key can be repeatedly acquired without causing a deadlock.
For example, after the command in the above example is executed, the command lock with the same key will not deadlock in the subsequent event processing process.