namespace NetCorePal.Extensions.CodeAnalysis;

public static class CallRelationshipTypes
{
    /// <summary>发出命令（如 Controller/Handler/Service 方法 → Command）</summary>
    public const string MethodToCommand = "MethodToCommand";
    /// <summary>命令调用聚合方法（如 Command → Aggregate.Method）</summary>
    public const string CommandToAggregateMethod = "CommandToAggregateMethod";
    /// <summary>聚合方法发出领域事件（如 Aggregate.Method → DomainEvent）</summary>
    public const string MethodToDomainEvent = "MethodToDomainEvent";
    /// <summary>领域事件处理器处理领域事件（如 DomainEvent → DomainEventHandler）</summary>
    public const string DomainEventToHandler = "DomainEventToHandler";
    /// <summary>集成事件处理器处理集成事件（如 IntegrationEvent → IntegrationEventHandler）</summary>
    public const string IntegrationEventToHandler = "IntegrationEventToHandler";
    /// <summary>领域事件转换为集成事件（如 DomainEvent → IntegrationEvent）</summary>
    public const string DomainEventToIntegrationEvent = "DomainEventToIntegrationEvent";
    /// <summary>事件处理器发出命令（如 Handler → Command）</summary>
    public const string HandlerToCommand = "HandlerToCommand";
    /// <summary>未知类型</summary>
    public const string Unknown = "Unknown";
}