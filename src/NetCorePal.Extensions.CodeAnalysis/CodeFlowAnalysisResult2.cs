using System.Collections.Generic;

namespace NetCorePal.Extensions.CodeAnalysis;

/// <summary>
/// 代码流分析结果（新版，统一抽象节点和关系）
/// </summary>
public class CodeFlowAnalysisResult2
{
    /// <summary>
    /// 所有节点（控制器方法、命令、聚合方法、领域事件、集成事件、处理器、命令发送者等）
    /// </summary>
    public List<Node> Nodes { get; set; } = new();

    /// <summary>
    /// 节点之间的关系
    /// </summary>
    public List<Relationship> Relationships { get; set; } = new();
}

/// <summary>
/// 节点类型枚举
/// </summary>
public enum NodeType
{
    Controller,
    ControllerMethod,
    Endpoint,
    CommandSender,
    CommandSenderMethod,
    Command,
    Entity,
    EntityMethod,
    DomainEvent,
    IntegrationEvent,
    DomainEventHandler,
    IntegrationEventHandler,
    IntegrationEventConverter
}

/// <summary>
/// 节点定义
/// </summary>
public class Node
{
    public string Id { get; set; } = string.Empty; // 唯一标识
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public NodeType Type { get; set; }
    public Dictionary<string, object>? Properties { get; set; } // 可选扩展属性
}

/// <summary>
/// 关系类型枚举
/// </summary>
public enum RelationshipType
{
    CommandSenderToCommand,
    CommandSenderMethodToCommand,
    ControllerToCommand,
    ControllerMethodToCommand,
    EndpointToCommand,
    CommandToEntityMethod,
    EntityMethodToDomainEvent,
    DomainEventToHandler,
    IntegrationEventToHandler,
    DomainEventToIntegrationEvent
}

/// <summary>
/// 节点之间的关系
/// </summary>
public class Relationship
{
    public Node? FromNode { get; set; } // 源节点对象
    public Node? ToNode { get; set; } // 目标节点对象
    public RelationshipType Type { get; set; }
}