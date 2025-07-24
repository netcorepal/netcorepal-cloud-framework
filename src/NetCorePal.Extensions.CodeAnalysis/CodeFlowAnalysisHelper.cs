using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetCorePal.Extensions.CodeAnalysis;
using NetCorePal.Extensions.CodeAnalysis.Attributes;

namespace NetCorePal.Extensions.CodeAnalysis;

/// <summary>
/// CodeFlowAnalysisResult 辅助分析工具类
/// </summary>
public static class CodeFlowAnalysisHelper
{
    /// <summary>
    /// 从指定程序集分析所有相关 MetadataAttribute，生成统一的 CodeFlowAnalysisResult
    /// </summary>
    /// <param name="assemblies">要分析的程序集集合</param>
    /// <returns>分析结果</returns>
    public static CodeFlowAnalysisResult2 AnalyzeFromAssemblies(params Assembly[] assemblies)
    {
        var result = new CodeFlowAnalysisResult2();
        var nodeDict = new Dictionary<string, Node>();
        var relationships = new List<Relationship>();

        // 1. 收集所有MetadataAttribute
        var allAttributes = new List<MetadataAttribute>();
        foreach (var assembly in assemblies)
        {
            allAttributes.AddRange(assembly.GetCustomAttributes().OfType<MetadataAttribute>());
        }

        // 2. 遍历所有MetadataAttribute收集所有类型的Node
        foreach (var attr in allAttributes)
        {
            if (attr is ControllerMetadataAttribute controllerAttr)
            {
                var nodeId = $"{controllerAttr.ControllerType}.{controllerAttr.ControllerMethodName}";
                var node = new Node
                {
                    Id = nodeId,
                    Name = controllerAttr.ControllerMethodName,
                    FullName = nodeId,
                    Type = NodeType.ControllerMethod
                };
                nodeDict[nodeId] = node;
            }
            else if (attr is EndpointMetadataAttribute endpointAttr)
            {
                var endpointNodeId = $"{endpointAttr.EndpointType}.{endpointAttr.EndpointMethodName}";
                var node = new Node
                {
                    Id = endpointNodeId,
                    Name = endpointAttr.EndpointMethodName ?? string.Empty,
                    FullName = endpointNodeId,
                    Type = NodeType.Endpoint
                };
                nodeDict[endpointNodeId] = node;
            }
            else if (attr is CommandMetadataAttribute commandAttr)
            {
                var node = new Node
                {
                    Id = commandAttr.CommandType,
                    Name = commandAttr.CommandType,
                    FullName = commandAttr.CommandType,
                    Type = NodeType.Command
                };
                nodeDict[commandAttr.CommandType] = node;
            }
            else if (attr is EntityMethodMetadataAttribute entityMethodAttr)
            {
                var nodeId = $"{entityMethodAttr.EntityType}.{entityMethodAttr.MethodName}";
                var node = new Node
                {
                    Id = nodeId,
                    Name = entityMethodAttr.MethodName,
                    FullName = nodeId,
                    Type = NodeType.EntityMethod
                };
                nodeDict[nodeId] = node;
            }
            else if (attr is CommandSenderMetadataAttribute senderAttr)
            {
                var nodeId = $"{senderAttr.SenderType}.{senderAttr.SenderMethodName}";
                var node = new Node
                {
                    Id = nodeId,
                    Name = senderAttr.SenderMethodName,
                    FullName = nodeId,
                    Type = NodeType.CommandSender
                };
                nodeDict[nodeId] = node;
            }
            else if (attr is CommandHandlerMetadataAttribute handlerAttr)
            {
                var node = new Node
                {
                    Id = handlerAttr.HandlerType,
                    Name = handlerAttr.HandlerType,
                    FullName = handlerAttr.HandlerType,
                    Type = NodeType.Command
                };
                nodeDict[handlerAttr.HandlerType] = node;
            }
            else if (attr is DomainEventMetadataAttribute domainEventAttr)
            {
                var node = new Node
                {
                    Id = domainEventAttr.EventType,
                    Name = domainEventAttr.EventType,
                    FullName = domainEventAttr.EventType,
                    Type = NodeType.DomainEvent
                };
                nodeDict[domainEventAttr.EventType] = node;
            }
            else if (attr is DomainEventHandlerMetadataAttribute domainHandlerAttr)
            {
                var node = new Node
                {
                    Id = domainHandlerAttr.HandlerType,
                    Name = domainHandlerAttr.HandlerType,
                    FullName = domainHandlerAttr.HandlerType,
                    Type = NodeType.DomainEventHandler
                };
                nodeDict[domainHandlerAttr.HandlerType] = node;
            }
            else if (attr is EntityMetadataAttribute entityAttr)
            {
                var node = new Node
                {
                    Id = entityAttr.EntityType,
                    Name = entityAttr.EntityType,
                    FullName = entityAttr.EntityType,
                    Type = NodeType.Entity
                };
                nodeDict[entityAttr.EntityType] = node;
            }
            else if (attr is IntegrationEventHandlerMetadataAttribute integrationHandlerAttr)
            {
                var node = new Node
                {
                    Id = integrationHandlerAttr.HandlerType,
                    Name = integrationHandlerAttr.HandlerType,
                    FullName = integrationHandlerAttr.HandlerType,
                    Type = NodeType.IntegrationEventHandler
                };
                nodeDict[integrationHandlerAttr.HandlerType] = node;
            }
            else if (attr is IntegrationEventConverterMetadataAttribute converterAttr)
            {
                // IntegrationEventConverter 节点
                var nodeId = $"{converterAttr.DomainEventType}->{converterAttr.IntegrationEventType}";
                var node = new Node
                {
                    Id = nodeId,
                    Name = nodeId,
                    FullName = nodeId,
                    Type = NodeType.IntegrationEventConverter
                };
                nodeDict[nodeId] = node;

                // 补充 IntegrationEvent 节点
                if (!string.IsNullOrEmpty(converterAttr.IntegrationEventType) && !nodeDict.ContainsKey(converterAttr.IntegrationEventType))
                {
                    nodeDict[converterAttr.IntegrationEventType] = new Node
                    {
                        Id = converterAttr.IntegrationEventType,
                        Name = converterAttr.IntegrationEventType,
                        FullName = converterAttr.IntegrationEventType,
                        Type = NodeType.IntegrationEvent
                    };
                }
                // 补充 DomainEvent 节点（防止漏掉）
                if (!string.IsNullOrEmpty(converterAttr.DomainEventType) && !nodeDict.ContainsKey(converterAttr.DomainEventType))
                {
                    nodeDict[converterAttr.DomainEventType] = new Node
                    {
                        Id = converterAttr.DomainEventType,
                        Name = converterAttr.DomainEventType,
                        FullName = converterAttr.DomainEventType,
                        Type = NodeType.DomainEvent
                    };
                }
            }
        }

        // 3. 遍历所有MetadataAttribute收集 EntityMethodToDomainEvent 关系
        foreach (var attr in allAttributes)
        {
            if (attr is EntityMethodMetadataAttribute entityMethodRelAttr)
            {
                var nodeId = $"{entityMethodRelAttr.EntityType}.{entityMethodRelAttr.MethodName}";
                var domainEventTypes = entityMethodRelAttr.EventTypes;
                if (domainEventTypes != null)
                {
                    var fromNode = nodeDict.ContainsKey(nodeId) ? nodeDict[nodeId] : null;
                    foreach (var evtType in domainEventTypes)
                    {
                        var evtNodeId = evtType ?? string.Empty;
                        var toNode = nodeDict.ContainsKey(evtNodeId) ? nodeDict[evtNodeId] : null;
                        if (fromNode != null && toNode != null)
                        {
                            relationships.Add(new Relationship
                            {
                                FromNode = fromNode,
                                ToNode = toNode,
                                Type = RelationshipType.EntityMethodToDomainEvent
                            });
                        }
                    }
                }
            }
        }

        // 3. 遍历所有MetadataAttribute收集所有的Relationships
        foreach (var attr in allAttributes)
        {
            if (attr is ControllerMetadataAttribute controllerAttr)
            {
                var nodeId = $"{controllerAttr.ControllerType}.{controllerAttr.ControllerMethodName}";
                var fromNode = nodeDict.ContainsKey(nodeId) ? nodeDict[nodeId] : null;
                // Command 关系
                foreach (var cmdType in controllerAttr.CommandTypes ?? Array.Empty<string>())
                {
                    var cmdNodeId = cmdType ?? string.Empty;
                    var toNode = nodeDict.ContainsKey(cmdNodeId) ? nodeDict[cmdNodeId] : null;
                    if (fromNode != null && toNode != null)
                    {
                        // 只保留 ControllerToCommand 关系
                        relationships.Add(new Relationship
                        {
                            FromNode = fromNode,
                            ToNode = toNode,
                            Type = RelationshipType.ControllerToCommand
                        });
                    }
                }
                // EndpointToCommand 关系收集
            }
            else if (attr is EndpointMetadataAttribute endpointAttr)
            {
                var endpointNodeId = $"{endpointAttr.EndpointType}.{endpointAttr.EndpointMethodName}";
                var endpointFromNode = nodeDict.ContainsKey(endpointNodeId) ? nodeDict[endpointNodeId] : null;
                foreach (var cmdType in endpointAttr.CommandTypes ?? Array.Empty<string>())
                {
                    var cmdNodeId = cmdType ?? string.Empty;
                    var toNode = nodeDict.ContainsKey(cmdNodeId) ? nodeDict[cmdNodeId] : null;
                    if (endpointFromNode != null && toNode != null)
                    {
                        relationships.Add(new Relationship
                        {
                            FromNode = endpointFromNode,
                            ToNode = toNode,
                            Type = RelationshipType.EndpointToCommand
                        });
                    }
                }
            }
            else if (attr is CommandSenderMetadataAttribute senderAttr)
            {
                var nodeId = $"{senderAttr.SenderType}.{senderAttr.SenderMethodName}";
                var fromNode = nodeDict.ContainsKey(nodeId) ? nodeDict[nodeId] : null;
                foreach (var cmdType in senderAttr.CommandTypes ?? Array.Empty<string>())
                {
                    var cmdNodeId = cmdType ?? string.Empty;
                    var toNode = nodeDict.ContainsKey(cmdNodeId) ? nodeDict[cmdNodeId] : null;
                    if (fromNode != null && toNode != null)
                    {
                        relationships.Add(new Relationship
                        {
                            FromNode = fromNode,
                            ToNode = toNode,
                            Type = RelationshipType.SenderMethodToCommand
                        });
                    }
                }
            }
            else if (attr is CommandHandlerMetadataAttribute handlerAttr)
            {
                var fromNode = nodeDict.ContainsKey(handlerAttr.HandlerType) ? nodeDict[handlerAttr.HandlerType] : null;
                if (!string.IsNullOrEmpty(handlerAttr.CommandType) && !string.IsNullOrEmpty(handlerAttr.EntityType) && !string.IsNullOrEmpty(handlerAttr.EntityMethodName))
                {
                    var entityMethodNodeId = $"{handlerAttr.EntityType}.{handlerAttr.EntityMethodName}";
                    var toNode = nodeDict.ContainsKey(entityMethodNodeId) ? nodeDict[entityMethodNodeId] : null;
                    if (fromNode != null && toNode != null)
                    {
                        relationships.Add(new Relationship
                        {
                            FromNode = fromNode,
                            ToNode = toNode,
                            Type = RelationshipType.CommandToEntityMethod
                        });
                    }
                }
            }
            else if (attr is DomainEventHandlerMetadataAttribute domainHandlerAttr)
            {
                var fromNode = nodeDict.ContainsKey(domainHandlerAttr.HandlerType) ? nodeDict[domainHandlerAttr.HandlerType] : null;
                var eventNode = nodeDict.ContainsKey(domainHandlerAttr.EventType) ? nodeDict[domainHandlerAttr.EventType] : null;
                if (eventNode != null && fromNode != null)
                {
                    relationships.Add(new Relationship
                    {
                        FromNode = eventNode,
                        ToNode = fromNode,
                        Type = RelationshipType.DomainEventToHandler
                    });
                }
                foreach (var cmdType in domainHandlerAttr.CommandTypes ?? Array.Empty<string>())
                {
                    var cmdNodeId = cmdType ?? string.Empty;
                    var toNode = nodeDict.ContainsKey(cmdNodeId) ? nodeDict[cmdNodeId] : null;
                    if (fromNode != null && toNode != null)
                    {
                        relationships.Add(new Relationship
                        {
                            FromNode = fromNode,
                            ToNode = toNode,
                            Type = RelationshipType.SenderMethodToCommand
                        });
                    }
                }
            }
            else if (attr is IntegrationEventHandlerMetadataAttribute integrationHandlerAttr)
            {
                var fromNode = nodeDict.ContainsKey(integrationHandlerAttr.HandlerType) ? nodeDict[integrationHandlerAttr.HandlerType] : null;
                var eventNode = nodeDict.ContainsKey(integrationHandlerAttr.EventType) ? nodeDict[integrationHandlerAttr.EventType] : null;
                if (eventNode != null && fromNode != null)
                {
                    relationships.Add(new Relationship
                    {
                        FromNode = eventNode,
                        ToNode = fromNode,
                        Type = RelationshipType.IntegrationEventToHandler
                    });
                }
                foreach (var cmdType in integrationHandlerAttr.CommandTypes ?? Array.Empty<string>())
                {
                    var cmdNodeId = cmdType ?? string.Empty;
                    var toNode = nodeDict.ContainsKey(cmdNodeId) ? nodeDict[cmdNodeId] : null;
                    if (fromNode != null && toNode != null)
                    {
                        relationships.Add(new Relationship
                        {
                            FromNode = fromNode,
                            ToNode = toNode,
                            Type = RelationshipType.SenderMethodToCommand
                        });
                    }
                }
            }
            else if (attr is IntegrationEventConverterMetadataAttribute converterAttr)
            {
                var domainNode = nodeDict.ContainsKey(converterAttr.DomainEventType) ? nodeDict[converterAttr.DomainEventType] : null;
                var integrationNode = nodeDict.ContainsKey(converterAttr.IntegrationEventType) ? nodeDict[converterAttr.IntegrationEventType] : null;
                if (domainNode != null && integrationNode != null)
                {
                    relationships.Add(new Relationship
                    {
                        FromNode = domainNode,
                        ToNode = integrationNode,
                        Type = RelationshipType.DomainEventToIntegrationEvent
                    });
                }
            }
        }

        result.Nodes = nodeDict.Values.ToList();
        result.Relationships = relationships;
        return result;
    }
}
