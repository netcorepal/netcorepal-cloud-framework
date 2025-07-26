using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetCorePal.Extensions.CodeAnalysis;
using NetCorePal.Extensions.CodeAnalysis.Attributes;

namespace NetCorePal.Extensions.CodeAnalysis;

public static class CodeFlowAnalysisHelper2
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
    
    public static CodeFlowAnalysisResult2 GetResultFromAssemblies(
        params Assembly[] assemblies)
    {
        var attributes = GetAllMetadataAttributes(assemblies);
        var nodes = new List<Node>();
        var relationships = new List<Relationship>();

        nodes.AddRange(GetControllerNodes(attributes));
        nodes.AddRange(GetControllerMethodNodes(attributes));
        nodes.AddRange(GetEndpointNodes(attributes));
        nodes.AddRange(GetCommandSenderNodes(attributes));
        nodes.AddRange(GetCommandSenderMethodNodes(attributes));
        nodes.AddRange(GetCommandNodes(attributes));
        nodes.AddRange(GetAggregateNodes(attributes));
        nodes.AddRange(GetAggregateMethodNodes(attributes));
        nodes.AddRange(GetDomainEventNodes(attributes));
        nodes.AddRange(GetIntegrationEventNodes(attributes));
        nodes.AddRange(GetDomainEventHandlerNodes(attributes));
        nodes.AddRange(GetIntegrationEventHandlerNodes(attributes));
        nodes.AddRange(GetIntegrationEventConverterNodes(attributes));

        relationships.AddRange(GetControllerToCommandRelationships(attributes));
        relationships.AddRange(GetControllerMethodToCommandRelationships(attributes));
        relationships.AddRange(GetEndpointToCommandRelationships(attributes));
        relationships.AddRange(GetCommandSenderToCommandRelationships(attributes));
        relationships.AddRange(GetCommandSenderMethodToCommandRelationships(attributes));
        relationships.AddRange(GetCommandToAggregateMethodRelationships(attributes));
        relationships.AddRange(GetAggregateMethodToDomainEventRelationships(attributes));
        relationships.AddRange(GetDomainEventToHandlerRelationships(attributes));
        relationships.AddRange(GetIntegrationEventToHandlerRelationships(attributes));
        relationships.AddRange(GetDomainEventToIntegrationEventRelationships(attributes));

        return new CodeFlowAnalysisResult2
        {
            Nodes = nodes,
            Relationships = relationships
        };
    }

    #region Get Nodes

    public static List<Node> GetControllerNodes(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<ControllerMetadataAttribute>().Select(attr => new Node
        {
            Id = attr.ControllerType,
            Name = GetClassName(attr.ControllerType),
            FullName = attr.ControllerType,
            Type = NodeType.Controller
        }).ToList();

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
        => attributes.OfType<CommandSenderMetadataAttribute>().Select(attr => new Node
        {
            Id = attr.SenderType,
            Name = GetClassName(attr.SenderType),
            FullName = attr.SenderType,
            Type = NodeType.CommandSender
        }).ToList();

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

    public static List<Relationship> GetControllerToCommandRelationships(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<ControllerMetadataAttribute>()
            .SelectMany(attr => attr.CommandTypes.Select(cmdType =>
                new Relationship(
                    new Node
                    {
                        Id = attr.ControllerType, Name = attr.ControllerType, FullName = attr.ControllerType,
                        Type = NodeType.Controller
                    },
                    new Node { Id = cmdType, Name = cmdType, FullName = cmdType, Type = NodeType.Command },
                    RelationshipType.ControllerToCommand
                ))).ToList();

    public static List<Relationship> GetControllerMethodToCommandRelationships(
        IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<ControllerMetadataAttribute>()
            .SelectMany(attr => attr.CommandTypes.Select(cmdType =>
                new Relationship(
                    new Node
                    {
                        Id = $"{attr.ControllerType}.{attr.ControllerMethodName}", Name = attr.ControllerMethodName,
                        FullName = $"{attr.ControllerType}.{attr.ControllerMethodName}", Type = NodeType.ControllerMethod
                    },
                    new Node { Id = cmdType, Name = cmdType, FullName = cmdType, Type = NodeType.Command },
                    RelationshipType.ControllerMethodToCommand
                ))).ToList();


    public static List<Relationship> GetEndpointToCommandRelationships(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<EndpointMetadataAttribute>()
            .SelectMany(attr => attr.CommandTypes.Select(cmdType =>
                new Relationship(
                    new Node
                    {
                        Id = $"{attr.EndpointType}.{attr.EndpointMethodName}", Name = attr.EndpointMethodName,
                        FullName = $"{attr.EndpointType}.{attr.EndpointMethodName}", Type = NodeType.Endpoint
                    },
                    new Node { Id = cmdType, Name = cmdType, FullName = cmdType, Type = NodeType.Command },
                    RelationshipType.EndpointToCommand
                ))).ToList();


    public static List<Relationship> GetCommandSenderToCommandRelationships(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<CommandSenderMetadataAttribute>()
            .SelectMany(attr => attr.CommandTypes.Select(cmdType =>
                new Relationship(
                    new Node
                    {
                        Id = attr.SenderType, Name = attr.SenderType, FullName = attr.SenderType,
                        Type = NodeType.CommandSender
                    },
                    new Node { Id = cmdType, Name = cmdType, FullName = cmdType, Type = NodeType.Command },
                    RelationshipType.CommandSenderToCommand
                ))).ToList();

    public static List<Relationship> GetCommandSenderMethodToCommandRelationships(
        IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<CommandSenderMetadataAttribute>()
            .SelectMany(attr => attr.CommandTypes.Select(cmdType =>
                new Relationship(
                    new Node
                    {
                        Id = $"{attr.SenderType}.{attr.SenderMethodName}", Name = attr.SenderMethodName,
                        FullName = $"{attr.SenderType}.{attr.SenderMethodName}", Type = NodeType.CommandSenderMethod
                    },
                    new Node { Id = cmdType, Name = cmdType, FullName = cmdType, Type = NodeType.Command },
                    RelationshipType.CommandSenderMethodToCommand
                ))).ToList();

    public static List<Relationship> GetCommandToAggregateMethodRelationships(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<CommandHandlerMetadataAttribute>()
            .Select(attr =>
                new Relationship(
                    new Node
                    {
                        Id = attr.HandlerType, Name = attr.HandlerType, FullName = attr.HandlerType, Type = NodeType.Command
                    },
                    new Node
                    {
                        Id = $"{attr.EntityType}.{attr.EntityMethodName}", Name = attr.EntityMethodName,
                        FullName = $"{attr.EntityType}.{attr.EntityMethodName}", Type = NodeType.AggregateMethod
                    },
                    RelationshipType.CommandToAggregateMethod
                )).ToList();

    public static List<Relationship> GetAggregateMethodToDomainEventRelationships(
        IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<EntityMethodMetadataAttribute>()
            .SelectMany(attr => (attr.EventTypes ?? Array.Empty<string>()).Select(evtType =>
                new Relationship(
                    new Node
                    {
                        Id = $"{attr.EntityType}.{attr.MethodName}", Name = attr.MethodName,
                        FullName = $"{attr.EntityType}.{attr.MethodName}", Type = NodeType.AggregateMethod
                    },
                    new Node { Id = evtType, Name = evtType, FullName = evtType, Type = NodeType.DomainEvent },
                    RelationshipType.AggregateMethodToDomainEvent
                ))).ToList();

    public static List<Relationship> GetDomainEventToHandlerRelationships(IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<DomainEventHandlerMetadataAttribute>()
            .Select(attr =>
                new Relationship(
                    new Node
                    {
                        Id = attr.EventType, Name = attr.EventType, FullName = attr.EventType, Type = NodeType.DomainEvent
                    },
                    new Node
                    {
                        Id = attr.HandlerType, Name = attr.HandlerType, FullName = attr.HandlerType,
                        Type = NodeType.DomainEventHandler
                    },
                    RelationshipType.DomainEventToHandler
                )).ToList();

    public static List<Relationship> GetIntegrationEventToHandlerRelationships(
        IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<IntegrationEventHandlerMetadataAttribute>()
            .Select(attr =>
                new Relationship(
                    new Node
                    {
                        Id = attr.EventType, Name = attr.EventType, FullName = attr.EventType,
                        Type = NodeType.IntegrationEvent
                    },
                    new Node
                    {
                        Id = attr.HandlerType, Name = attr.HandlerType, FullName = attr.HandlerType,
                        Type = NodeType.IntegrationEventHandler
                    },
                    RelationshipType.IntegrationEventToHandler
                )).ToList();

    public static List<Relationship> GetDomainEventToIntegrationEventRelationships(
        IEnumerable<MetadataAttribute> attributes)
        => attributes.OfType<IntegrationEventConverterMetadataAttribute>()
            .Select(attr =>
                new Relationship(
                    new Node
                    {
                        Id = attr.DomainEventType, Name = attr.DomainEventType, FullName = attr.DomainEventType,
                        Type = NodeType.DomainEvent
                    },
                    new Node
                    {
                        Id = attr.IntegrationEventType, Name = attr.IntegrationEventType,
                        FullName = attr.IntegrationEventType, Type = NodeType.IntegrationEvent
                    },
                    RelationshipType.DomainEventToIntegrationEvent
                )).ToList();

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