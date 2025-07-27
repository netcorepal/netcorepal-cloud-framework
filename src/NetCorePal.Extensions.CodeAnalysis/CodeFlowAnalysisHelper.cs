using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetCorePal.Extensions.CodeAnalysis;
using NetCorePal.Extensions.CodeAnalysis.Attributes;

namespace NetCorePal.Extensions.CodeAnalysis;

public static class CodeFlowAnalysisHelper
{
    
    /// <summary>
    /// 从程序集集合获取所有MetadataAttribute
    /// </summary>
    public static List<MetadataAttribute> GetAllMetadataAttributes(params Assembly[] assemblies)
    {
        var allAttributes = new List<MetadataAttribute>();
        foreach (var assembly in assemblies)
        {
            allAttributes.AddRange(assembly.GetCustomAttributes().OfType<MetadataAttribute>());
        }

        return allAttributes;
    }
    
    public static CodeFlowAnalysisResult GetResultFromAssemblies(
        params Assembly[] assemblies)
    {
        var attributes = GetAllMetadataAttributes(assemblies);
        var nodes = new List<Node>();

        var controllerNodes = GetControllerNodes(attributes);
        var controllerMethodNodes = GetControllerMethodNodes(attributes);
        var endpointNodes = GetEndpointNodes(attributes);
        var commandSenderNodes = GetCommandSenderNodes(attributes);
        var commandSenderMethodNodes = GetCommandSenderMethodNodes(attributes);
        var commandNodes = GetCommandNodes(attributes);
        var commandHandlerNodes = GetCommandHandlerNodes(attributes);
        var aggregateNodes = GetAggregateNodes(attributes);
        var aggregateMethodNodes = GetAggregateMethodNodes(attributes);
        var domainEventNodes = GetDomainEventNodes(attributes);
        var integrationEventNodes = GetIntegrationEventNodes(attributes);
        var domainEventHandlerNodes = GetDomainEventHandlerNodes(attributes);
        var integrationEventHandlerNodes = GetIntegrationEventHandlerNodes(attributes);
        var integrationEventConverterNodes = GetIntegrationEventConverterNodes(attributes);

        nodes.AddRange(controllerNodes);
        nodes.AddRange(controllerMethodNodes);
        nodes.AddRange(endpointNodes);
        nodes.AddRange(commandSenderNodes);
        nodes.AddRange(commandSenderMethodNodes);
        nodes.AddRange(commandNodes);
        nodes.AddRange(commandHandlerNodes);
        nodes.AddRange(aggregateNodes);
        nodes.AddRange(aggregateMethodNodes);
        nodes.AddRange(domainEventNodes);
        nodes.AddRange(integrationEventNodes);
        nodes.AddRange(domainEventHandlerNodes);
        nodes.AddRange(integrationEventHandlerNodes);
        nodes.AddRange(integrationEventConverterNodes);

        var relationships = new List<Relationship>();
        relationships.AddRange(GetControllerToCommandRelationships(controllerNodes, commandNodes, attributes));
        relationships.AddRange(GetControllerMethodToCommandRelationships(controllerMethodNodes, commandNodes));
        relationships.AddRange(GetEndpointToCommandRelationships(endpointNodes, commandNodes, attributes));
        relationships.AddRange(GetCommandSenderToCommandRelationships(commandSenderNodes, commandNodes));
        relationships.AddRange(GetCommandSenderMethodToCommandRelationships(commandSenderMethodNodes, commandNodes));
        relationships.AddRange(GetCommandToAggregateRelationships(commandNodes, aggregateNodes, attributes));
        relationships.AddRange(GetCommandToAggregateMethodRelationships(commandNodes, aggregateMethodNodes));
        relationships.AddRange(GetAggregateMethodToDomainEventRelationships(aggregateMethodNodes, domainEventNodes));
        relationships.AddRange(GetDomainEventToHandlerRelationships(domainEventNodes, domainEventHandlerNodes));
        relationships.AddRange(
            GetIntegrationEventToHandlerRelationships(integrationEventNodes, integrationEventHandlerNodes));
        relationships.AddRange(GetDomainEventToIntegrationEventRelationships(domainEventNodes, integrationEventNodes));

        // 去重：每对 fromNode.Id, toNode.Id, type 只出现一次
        var uniqueRelationships = relationships
            .GroupBy(r => (r.FromNode.Id, r.ToNode.Id, r.Type))
            .Select(g => g.First())
            .ToList();

        return new CodeFlowAnalysisResult
        {
            Nodes = nodes,
            Relationships = uniqueRelationships
        };
    }

    #region Get Nodes

    public static List<Node> GetControllerNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<ControllerMetadataAttribute>()
            .GroupBy(attr => attr.ControllerType)
            .Select(g =>
                new Node
                {
                    Id = g.Key,
                    Name = GetClassName(g.Key),
                    FullName = g.Key,
                    Type = NodeType.Controller
                })
            .ToList();

    public static List<Node> GetControllerMethodNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<ControllerMetadataAttribute>().Select(attr => new Node
        {
            Id = $"{attr.ControllerType}.{attr.ControllerMethodName}",
            Name = attr.ControllerMethodName,
            FullName = $"{attr.ControllerType}.{attr.ControllerMethodName}",
            Type = NodeType.ControllerMethod
        }).ToList();

    public static List<Node> GetEndpointNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<EndpointMetadataAttribute>().Select(attr => new Node
        {
            Id = $"{attr.EndpointType}.{attr.EndpointMethodName}",
            Name = GetClassName(attr.EndpointType),
            FullName = $"{attr.EndpointType}.{attr.EndpointMethodName}",
            Type = NodeType.Endpoint
        }).ToList();


    public static List<Node> GetCommandSenderNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<CommandSenderMetadataAttribute>()
            .GroupBy(attr => attr.SenderType)
            .Select(g =>
                new Node
                {
                    Id = g.Key,
                    Name = GetClassName(g.Key),
                    FullName = g.Key,
                    Type = NodeType.CommandSender
                })
            .ToList();

    public static List<Node> GetCommandSenderMethodNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<CommandSenderMetadataAttribute>().Select(attr => new Node
        {
            Id = $"{attr.SenderType}.{attr.SenderMethodName}",
            Name = attr.SenderMethodName,
            FullName = $"{attr.SenderType}.{attr.SenderMethodName}",
            Type = NodeType.CommandSenderMethod
        }).ToList();


    public static List<Node> GetCommandNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<CommandMetadataAttribute>().Select(attr => new Node
        {
            Id = attr.CommandType,
            Name = GetClassName(attr.CommandType),
            FullName = attr.CommandType,
            Type = NodeType.Command
        }).ToList();

    
    public static List<Node> GetCommandHandlerNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<CommandHandlerMetadataAttribute>()
            .GroupBy(attr => attr.HandlerType)
            .Select(g =>
                new Node
                {
                    Id = g.Key,
                    Name = GetClassName(g.Key),
                    FullName = g.Key,
                    Type = NodeType.CommandHandler
                })
            .ToList();
    public static List<Node> GetAggregateNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<EntityMetadataAttribute>().Where(p => p.IsAggregateRoot).Select(attr => new Node
        {
            Id = attr.EntityType,
            Name = GetClassName(attr.EntityType),
            FullName = attr.EntityType,
            Type = NodeType.Aggregate
        }).ToList();

    public static List<Node> GetAggregateMethodNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<EntityMethodMetadataAttribute>().Select(attr => new Node
        {
            Id = $"{attr.EntityType}.{attr.MethodName}",
            Name = attr.MethodName,
            FullName = $"{attr.EntityType}.{attr.MethodName}",
            Type = NodeType.AggregateMethod
        }).ToList();


    public static List<Node> GetDomainEventNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<DomainEventMetadataAttribute>().Select(attr => new Node
        {
            Id = attr.EventType,
            Name = GetClassName(attr.EventType),
            FullName = attr.EventType,
            Type = NodeType.DomainEvent
        }).ToList();

    public static List<Node> GetIntegrationEventNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<IntegrationEventConverterMetadataAttribute>().Select(attr => new Node
        {
            Id = attr.IntegrationEventType,
            Name = GetClassName(attr.IntegrationEventType),
            FullName = attr.IntegrationEventType,
            Type = NodeType.IntegrationEvent
        }).ToList();

    public static List<Node> GetDomainEventHandlerNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<DomainEventHandlerMetadataAttribute>().Select(attr => new Node
        {
            Id = attr.HandlerType,
            Name = GetClassName(attr.HandlerType),
            FullName = attr.HandlerType,
            Type = NodeType.DomainEventHandler
        }).ToList();

    public static List<Node> GetIntegrationEventHandlerNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<IntegrationEventHandlerMetadataAttribute>().Select(attr => new Node
        {
            Id = attr.HandlerType,
            Name = GetClassName(attr.HandlerType),
            FullName = attr.HandlerType,
            Type = NodeType.IntegrationEventHandler
        }).ToList();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="attributes"></param>
    /// <returns></returns>
    public static List<Node> GetIntegrationEventConverterNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<IntegrationEventConverterMetadataAttribute>().Select(attr => new Node
        {
            Id = $"{attr.DomainEventType}->{attr.IntegrationEventType}",
            Name = $"{GetClassName(attr.DomainEventType)}To{GetClassName(attr.IntegrationEventType)}",
            FullName = $"{attr.DomainEventType}->{attr.IntegrationEventType}",
            Type = NodeType.IntegrationEventConverter
        }).ToList();

    #endregion

    #region Relationships

    /// <summary>
    /// 获取 Command 到 Aggregate 的真实关系（通过 EntityMetadataAttribute 和 EntityMethodMetadataAttribute 分析）
    /// </summary>
    /// <param name="fromNodes"></param>
    /// <param name="toNodes"></param>
    /// <param name="attributes"></param>
    /// <returns></returns>
    public static List<Relationship> GetCommandToAggregateRelationships(IEnumerable<Node> fromNodes,
        IEnumerable<Node> toNodes, IEnumerable<MetadataAttribute> attributes)
    {
        // 直接通过 CommandHandlerMetadataAttribute 建立命令与聚合根的关系
        var handlerAttrs = attributes.OfType<CommandHandlerMetadataAttribute>().ToList();
        var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.Command && n.Id != null)
            .ToDictionary(n => n.Id, n => n);
        var toNodeDict = toNodes.Where(n => n.Type == NodeType.Aggregate && n.Id != null)
            .ToDictionary(n => n.Id, n => n);

        var relationships = new List<Relationship>();
        foreach (var handlerAttr in handlerAttrs)
        {
            var commandType = handlerAttr.CommandType;
            if (handlerAttr.AggregateTypes != null)
            {
                foreach (var aggType in handlerAttr.AggregateTypes)
                {
                    if (fromNodeDict.TryGetValue(commandType, out var fromNode) &&
                        toNodeDict.TryGetValue(aggType, out var toNode))
                    {
                        relationships.Add(new Relationship(fromNode, toNode, RelationshipType.CommandToAggregate));
                    }
                }
            }
        }
        // 去重：每对 fromNode.Id, toNode.Id, type 只出现一次
        var uniqueRelationships = relationships
            .GroupBy(r => (r.FromNode.Id, r.ToNode.Id, r.Type))
            .Select(g => g.First())
            .ToList();
        return uniqueRelationships;
    }

    public static List<Relationship> GetControllerToCommandRelationships(IEnumerable<Node> fromNodes,
        IEnumerable<Node> toNodes, IEnumerable<MetadataAttribute> attributes)
    {
        // 只生成 ControllerMetadataAttribute 中实际存在的 Controller-Command 关系
        var controllerAttrs = attributes.OfType<ControllerMetadataAttribute>();
        var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.Controller && n.Id != null)
            .ToDictionary(n => n.Id, n => n);
        var toNodeDict = toNodes.Where(n => n.Type == NodeType.Command && n.Id != null)
            .ToDictionary(n => n.Id, n => n);

        var relationships = new List<Relationship>();
        foreach (var attr in controllerAttrs)
        {
            if (fromNodeDict.TryGetValue(attr.ControllerType, out var fromNode) && attr.CommandTypes != null)
            {
                foreach (var cmdType in attr.CommandTypes)
                {
                    if (toNodeDict.TryGetValue(cmdType, out var toNode))
                    {
                        relationships.Add(new Relationship(fromNode, toNode, RelationshipType.ControllerToCommand));
                    }
                }
            }
        }

        return relationships;
    }

    public static List<Relationship> GetControllerMethodToCommandRelationships(IEnumerable<Node> fromNodes,
        IEnumerable<Node> toNodes)
        => (from fromNode in fromNodes.Where(n => n.Type == NodeType.ControllerMethod)
            from toNode in toNodes.Where(n => n.Type == NodeType.Command)
            where fromNode.Id != null && toNode.Id != null
            select new Relationship(fromNode, toNode, RelationshipType.ControllerMethodToCommand)).ToList();

    public static List<Relationship> GetEndpointToCommandRelationships(IEnumerable<Node> fromNodes,
        IEnumerable<Node> toNodes, IEnumerable<MetadataAttribute> attributes)
    {
        // 只生成 EndpointMetadataAttribute 中实际存在的 Endpoint-Command 关系
        var endpointAttrs = attributes.OfType<EndpointMetadataAttribute>();
        var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.Endpoint && n.Id != null)
            .ToDictionary(n => n.Id, n => n);
        var toNodeDict = toNodes.Where(n => n.Type == NodeType.Command && n.Id != null)
            .ToDictionary(n => n.Id, n => n);

        var relationships = new List<Relationship>();
        foreach (var attr in endpointAttrs)
        {
            var endpointId = $"{attr.EndpointType}.{attr.EndpointMethodName}";
            if (fromNodeDict.TryGetValue(endpointId, out var fromNode) && attr.CommandTypes != null)
            {
                foreach (var cmdType in attr.CommandTypes)
                {
                    if (toNodeDict.TryGetValue(cmdType, out var toNode))
                    {
                        relationships.Add(new Relationship(fromNode, toNode, RelationshipType.EndpointToCommand));
                    }
                }
            }
        }

        return relationships;
    }

    public static List<Relationship> GetCommandSenderToCommandRelationships(IEnumerable<Node> fromNodes,
        IEnumerable<Node> toNodes)
        => (from fromNode in fromNodes.Where(n => n.Type == NodeType.CommandSender)
            from toNode in toNodes.Where(n => n.Type == NodeType.Command)
            where fromNode.Id != null && toNode.Id != null
            select new Relationship(fromNode, toNode, RelationshipType.CommandSenderToCommand)).ToList();

    public static List<Relationship> GetCommandSenderMethodToCommandRelationships(IEnumerable<Node> fromNodes,
        IEnumerable<Node> toNodes)
        => (from fromNode in fromNodes.Where(n => n.Type == NodeType.CommandSenderMethod)
            from toNode in toNodes.Where(n => n.Type == NodeType.Command)
            where fromNode.Id != null && toNode.Id != null
            select new Relationship(fromNode, toNode, RelationshipType.CommandSenderMethodToCommand)).ToList();

    public static List<Relationship> GetCommandToAggregateMethodRelationships(IEnumerable<Node> fromNodes,
        IEnumerable<Node> toNodes)
        => (from fromNode in fromNodes.Where(n => n.Type == NodeType.Command)
            from toNode in toNodes.Where(n => n.Type == NodeType.AggregateMethod)
            where fromNode.Id != null && toNode.Id != null
            select new Relationship(fromNode, toNode, RelationshipType.CommandToAggregateMethod)).ToList();

    public static List<Relationship> GetAggregateMethodToDomainEventRelationships(IEnumerable<Node> fromNodes,
        IEnumerable<Node> toNodes)
        => (from fromNode in fromNodes.Where(n => n.Type == NodeType.AggregateMethod)
            from toNode in toNodes.Where(n => n.Type == NodeType.DomainEvent)
            where fromNode.Id != null && toNode.Id != null
            select new Relationship(fromNode, toNode, RelationshipType.AggregateMethodToDomainEvent)).ToList();

    public static List<Relationship> GetDomainEventToHandlerRelationships(IEnumerable<Node> fromNodes,
        IEnumerable<Node> toNodes)
        => (from fromNode in fromNodes.Where(n => n.Type == NodeType.DomainEvent)
            from toNode in toNodes.Where(n => n.Type == NodeType.DomainEventHandler)
            where fromNode.Id != null && toNode.Id != null
            select new Relationship(fromNode, toNode, RelationshipType.DomainEventToHandler)).ToList();

    public static List<Relationship> GetIntegrationEventToHandlerRelationships(IEnumerable<Node> fromNodes,
        IEnumerable<Node> toNodes)
        => (from fromNode in fromNodes.Where(n => n.Type == NodeType.IntegrationEvent)
            from toNode in toNodes.Where(n => n.Type == NodeType.IntegrationEventHandler)
            where fromNode.Id != null && toNode.Id != null
            select new Relationship(fromNode, toNode, RelationshipType.IntegrationEventToHandler)).ToList();

    public static List<Relationship> GetDomainEventToIntegrationEventRelationships(IEnumerable<Node> fromNodes,
        IEnumerable<Node> toNodes)
        => (from fromNode in fromNodes.Where(n => n.Type == NodeType.DomainEvent)
            from toNode in toNodes.Where(n => n.Type == NodeType.IntegrationEvent)
            where fromNode.Id != null && toNode.Id != null
            select new Relationship(fromNode, toNode, RelationshipType.DomainEventToIntegrationEvent)).ToList();

    #endregion


    #region 私有方法

    private static string GetClassName(string fullTypeName)
    {
        if (string.IsNullOrEmpty(fullTypeName)) return string.Empty;
        var lastDot = fullTypeName.LastIndexOf('.');
        return lastDot >= 0 ? fullTypeName.Substring(lastDot + 1) : fullTypeName;
    }

    #endregion
}