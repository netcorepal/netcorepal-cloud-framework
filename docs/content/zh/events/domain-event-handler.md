# 领域事件处理

领域事件处理器是一个因`特定目的`而处理`领域事件`的处理逻辑，一个领域事件处理器应该仅针对一个目的来处理领域事件，针对同一`领域事件`的不同目的应该有不同的领域事件处理器。

## 定义领域事件处理器


1. 安装nuget包 `NetCorePal.Extensions.Domain.Abstractions`

    ```bash
    dotnet add package NetCorePal.Extensions.Domain.Abstractions
    ```

2. 领域事件处理器是一个实现了`IDomainEventHandler<TDomainEvent>`接口的类，其中`TDomainEvent`是领域事件的类型。

下面是一个领域事件处理器的例子：

    ```csharp
    public class OrderCreatedDomainEventHandler(IMediator mediator) : IDomainEventHandler<OrderCreatedDomainEvent>
    {
        public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            return mediator.Send(new DeliverGoodsCommand(notification.Order.Id), cancellationToken);
        }
    }
    ```

## 领域事件处理器必须

- 领域事件处理器必须是幂等的，即多次处理同一领域事件，结果应该是一致的。
- 领域事件处理器必须是无状态的，即不应该有任何状态，所有的状态应该通过领域事件传递。

**备注： 在我们的框架中，领域事件处理器是同步执行的，并且其调用的Command与触发领域事件的CommandHandler处于同一事务中**

## 领域事件处理器可以

- 领域事件处理器中可以使用`MediatR`框架来发送命令；
- 领域事件处理器中可以做一些简单的数据转换和信息查询；
- 领域事件处理器中可以调用外部服务来完成一些信息组织和验证；
- 领域事件处理器中可以发出集成事件来将事件传递给其它系统；

## 领域事件处理器不要

- 领域事件处理器中不要包含领域模型的操作，应该由CommandHandler操作领域模型并持久化；

**备注： 由于我们框架仅对`CommandHandler`做了事务管理，对于领域事件处理器中的操作框架不会做`SaveChangesAsync`，从而导致领域事件处理器中的操作不会被保存到数据库中**