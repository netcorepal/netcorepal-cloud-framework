# Transactions

## Domain Events and Transactions

`Domain events` and `commands` run in the same database transaction, achieving `strong consistency`. If a `domain event handler` throws an exception, the transaction will roll back, and the operations of both the `command` and the `domain event` will roll back.

## Integration Events and Transactions

The handling of `integration events` currently achieves `eventual consistency` based on the `Outbox pattern` of the CAP framework.

The saving of `integration events` and `commands` run in the same database transaction. Once the transaction is committed, the `integration events` will also be saved to the database by the `CAP` component and sent to the `MQ`.

The handling of `integration events` by the `integration event handler` is in a different transaction from the `command` that initiated it. If the `integration event handler` throws an exception, the `integration event` will be recorded and retried by the `CAP` component, and the operations of the `command` will not roll back.

## Transaction Lifecycle

The following diagram shows the lifecycle of a transaction:

[![transactions](../img/transactions.png)](../img/transactions.png)

The framework starts a transaction before the `CommandHandler` begins. The `DomainEvent` issued by the `CommandHandler` operating on the domain model will be handled by the `DomainEventHandler`. After processing, the transaction will be committed. If an exception occurs in the `CommandHandler` or `DomainEventHandler`, the transaction will roll back.

If the `DomainEventHandler` converts the `DomainEvent` to an `IntegrationEvent`, the `IntegrationEvent` will be persisted to the database in the current database transaction. If the persistence fails, an exception will be thrown, and the transaction will roll back.

If the database transaction is successfully committed, the execution result of the `CommandHandler` will be returned, the current request processing will be completed, and the `IntegrationEvent` will be published to the message queue.

The `IntegrationEvent` will be handled by the `IntegrationEventHandler` that subscribes to it. After processing, the transaction will be committed. If an exception occurs in the `IntegrationEventHandler`, the framework will log the exception and attempt to retry. By default, it will retry 10 times with a certain interval between each retry.


