# 事务

## 领域事件与事务

`领域事件`与`命令`会运行在同一个数据库事务中，即实现了`强一致性`，如果`领域事件处理器`抛出异常，事务会回滚，`命令`和`领域事件`的操作都会回滚。

## 集成事件与事务

`集成事件`的处理目前基于CAP框架的`Outbox模型`实现了`最终一致性`。

`集成事件`的保存与`命令`会运行在同一个数据库事务中，一旦事务提交，`集成事件`也会被`CAP`组件保存到数据库中，并发送到`MQ`中。

`集成事件处理器`的处理与发起它的`命令`是在不同的事务中，如果`集成事件处理器`抛出异常，`集成事件`会被`CAP`组件记录并重试，`命令`的操作不会回滚。

## 事务生命周期

下图展示了一个事务的生命周期：

[![transactions](../img/transactions.png)](../img/transactions.png)

框架会在 `CommandHandler` 开始前开启一个事务，由 `CommandHandler` 操作领域模型而发出的`DomainEvent` 会被`DomainEventHandler`处理，处理完成后会提交事务。如果在`CommandHandler`或`DomainEventHandler`中发生异常，事务会回滚。

如果在`DomainEventHandler`中将`DomainEvent`转换为`IntegrationEvent`，则会在当前数据库事务中将`IntegrationEvent`持久化到数据库，如果持久化失败，会抛出异常，事务会回滚。

如果数据库事务提交成功，则会返回`CommandHandler`的执行结果，当前请求处理完成，同时会发布`IntegrationEvent`到消息队列。

`IntegrationEvent`会被订阅它地`IntegrationEventHandler`处理，处理完成后会提交事务。如果在`IntegrationEventHandler`中发生异常，框架会记录异常日志，并尝试重试，默认情况下会重试10次，每次重试会有一定地时间间隔。


