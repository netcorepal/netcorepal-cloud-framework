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
                // 添加 Controller 类型 Node
                var controllerNodeId = controllerAttr.ControllerType;
                if (!nodeDict.ContainsKey(controllerNodeId))
                {
                    nodeDict[controllerNodeId] = new Node
                    {
                        Id = controllerNodeId,
                        Name = GetClassName(controllerAttr.ControllerType),
                        FullName = controllerAttr.ControllerType,
                        Type = NodeType.Controller
                    };
                }

                // 添加 ControllerMethod 类型 Node
                var nodeId = $"{controllerAttr.ControllerType}.{controllerAttr.ControllerMethodName}";
                nodeDict[nodeId] = new Node
                {
                    Id = nodeId,
                    Name = controllerAttr.ControllerMethodName,
                    FullName = nodeId,
                    Type = NodeType.ControllerMethod
                };
            }
            else if (attr is EndpointMetadataAttribute endpointAttr)
            {
                var endpointNodeId = $"{endpointAttr.EndpointType}.{endpointAttr.EndpointMethodName}";
                nodeDict[endpointNodeId] = new Node
                {
                    Id = endpointNodeId,
                    Name = GetClassName(endpointAttr.EndpointType),
                    FullName = endpointNodeId,
                    Type = NodeType.Endpoint
                };
            }
            else if (attr is CommandMetadataAttribute commandAttr)
            {
                nodeDict[commandAttr.CommandType] = new Node
                {
                    Id = commandAttr.CommandType,
                    Name = GetClassName(commandAttr.CommandType),
                    FullName = commandAttr.CommandType,
                    Type = NodeType.Command
                };
            }
            else if (attr is EntityMethodMetadataAttribute entityMethodAttr)
            {
                // 只收集 AggregateMethod 类型 Node
                var aggregateMethodNodeId = $"{entityMethodAttr.EntityType}.{entityMethodAttr.MethodName}";
                nodeDict[aggregateMethodNodeId] = new Node
                {
                    Id = aggregateMethodNodeId,
                    Name = entityMethodAttr.MethodName,
                    FullName = aggregateMethodNodeId,
                    Type = NodeType.AggregateMethod
                };
            }
            else if (attr is CommandSenderMetadataAttribute senderAttr)
            {
                // 添加 CommandSender 类型 Node
                var senderNodeId = senderAttr.SenderType;
                if (!nodeDict.ContainsKey(senderNodeId))
                {
                    nodeDict[senderNodeId] = new Node
                    {
                        Id = senderNodeId,
                        Name = GetClassName(senderAttr.SenderType),
                        FullName = senderAttr.SenderType,
                        Type = NodeType.CommandSender
                    };
                }

                // 添加 CommandSenderMethod 类型 Node
                var senderMethodNodeId = $"{senderAttr.SenderType}.{senderAttr.SenderMethodName}";
                nodeDict[senderMethodNodeId] = new Node
                {
                    Id = senderMethodNodeId,
                    Name = senderAttr.SenderMethodName,
                    FullName = senderMethodNodeId,
                    Type = NodeType.CommandSenderMethod
                };
            }
            else if (attr is CommandHandlerMetadataAttribute handlerAttr)
            {
                nodeDict[handlerAttr.HandlerType] = new Node
                {
                    Id = handlerAttr.HandlerType,
                    Name = GetClassName(handlerAttr.HandlerType),
                    FullName = handlerAttr.HandlerType,
                    Type = NodeType.Command
                };
            }
            else if (attr is DomainEventMetadataAttribute domainEventAttr)
            {
                nodeDict[domainEventAttr.EventType] = new Node
                {
                    Id = domainEventAttr.EventType,
                    Name = GetClassName(domainEventAttr.EventType),
                    FullName = domainEventAttr.EventType,
                    Type = NodeType.DomainEvent
                };
            }
            else if (attr is DomainEventHandlerMetadataAttribute domainHandlerAttr)
            {
                nodeDict[domainHandlerAttr.HandlerType] = new Node
                {
                    Id = domainHandlerAttr.HandlerType,
                    Name = GetClassName(domainHandlerAttr.HandlerType),
                    FullName = domainHandlerAttr.HandlerType,
                    Type = NodeType.DomainEventHandler
                };
            }
            else if (attr is EntityMetadataAttribute entityAttr)
            {
                // 只收集 Aggregate 类型 Node
                var aggregateNodeId = entityAttr.EntityType;
                if (!nodeDict.ContainsKey(aggregateNodeId))
                {
                    nodeDict[aggregateNodeId] = new Node
                    {
                        Id = aggregateNodeId,
                        Name = GetClassName(entityAttr.EntityType),
                        FullName = entityAttr.EntityType,
                        Type = NodeType.Aggregate
                    };
                }
                // 推断子实体的方法属于聚合方法
                if (entityAttr.SubEntities != null)
                {
                    foreach (var childEntityType in entityAttr.SubEntities)
                    {
                        // 查找所有属于该子实体的方法
                        foreach (var methodAttr in allAttributes.OfType<EntityMethodMetadataAttribute>().Where(m => m.EntityType == childEntityType))
                        {
                            var aggregateMethodNodeId = $"{childEntityType}.{methodAttr.MethodName}";
                            nodeDict[aggregateMethodNodeId] = new Node
                            {
                                Id = aggregateMethodNodeId,
                                Name = methodAttr.MethodName,
                                FullName = aggregateMethodNodeId,
                                Type = NodeType.AggregateMethod
                            };
                        }
                    }
                }
            }
            else if (attr is IntegrationEventHandlerMetadataAttribute integrationHandlerAttr)
            {
                nodeDict[integrationHandlerAttr.HandlerType] = new Node
                {
                    Id = integrationHandlerAttr.HandlerType,
                    Name = GetClassName(integrationHandlerAttr.HandlerType),
                    FullName = integrationHandlerAttr.HandlerType,
                    Type = NodeType.IntegrationEventHandler
                };
            }
            else if (attr is IntegrationEventConverterMetadataAttribute converterAttr)
            {
                // IntegrationEventConverter 节点
                var nodeId = $"{converterAttr.DomainEventType}->{converterAttr.IntegrationEventType}";
                nodeDict[nodeId] = new Node
                {
                    Id = nodeId,
                    Name = GetClassName(converterAttr.DomainEventType),
                    FullName = nodeId,
                    Type = NodeType.IntegrationEventConverter
                };

                // 补充 IntegrationEvent 节点
                if (!string.IsNullOrEmpty(converterAttr.IntegrationEventType) &&
                    !nodeDict.ContainsKey(converterAttr.IntegrationEventType))
                {
                    nodeDict[converterAttr.IntegrationEventType] = new Node
                    {
                        Id = converterAttr.IntegrationEventType,
                        Name = GetClassName(converterAttr.IntegrationEventType),
                        FullName = converterAttr.IntegrationEventType,
                        Type = NodeType.IntegrationEvent
                    };
                }

                // 补充 DomainEvent 节点（防止漏掉）
                if (!string.IsNullOrEmpty(converterAttr.DomainEventType) &&
                    !nodeDict.ContainsKey(converterAttr.DomainEventType))
                {
                    nodeDict[converterAttr.DomainEventType] = new Node
                    {
                        Id = converterAttr.DomainEventType,
                        Name = GetClassName(converterAttr.DomainEventType),
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
                // ControllerMethodToCommand 关系
                var controllerMethodNodeId = $"{controllerAttr.ControllerType}.{controllerAttr.ControllerMethodName}";
                var controllerMethodNode = nodeDict.ContainsKey(controllerMethodNodeId)
                    ? nodeDict[controllerMethodNodeId]
                    : null;
                foreach (var cmdType in controllerAttr.CommandTypes ?? Array.Empty<string>())
                {
                    var cmdNodeId = cmdType ?? string.Empty;
                    var toNode = nodeDict.ContainsKey(cmdNodeId) ? nodeDict[cmdNodeId] : null;
                    if (controllerMethodNode != null && toNode != null)
                    {
                        relationships.Add(new Relationship
                        {
                            FromNode = controllerMethodNode,
                            ToNode = toNode,
                            Type = RelationshipType.ControllerMethodToCommand
                        });
                    }
                }

                // ControllerToCommand 关系（Controller节点到Command节点）
                var controllerNodeId = controllerAttr.ControllerType;
                var controllerNode = nodeDict.ContainsKey(controllerNodeId) ? nodeDict[controllerNodeId] : null;
                foreach (var cmdType in controllerAttr.CommandTypes ?? Array.Empty<string>())
                {
                    var cmdNodeId = cmdType ?? string.Empty;
                    var toNode = nodeDict.ContainsKey(cmdNodeId) ? nodeDict[cmdNodeId] : null;
                    if (controllerNode != null && toNode != null)
                    {
                        relationships.Add(new Relationship
                        {
                            FromNode = controllerNode,
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
                // CommandSenderMethodToCommand 关系
                var senderMethodNodeId = $"{senderAttr.SenderType}.{senderAttr.SenderMethodName}";
                var senderMethodNode = nodeDict.ContainsKey(senderMethodNodeId) ? nodeDict[senderMethodNodeId] : null;
                foreach (var cmdType in senderAttr.CommandTypes ?? Array.Empty<string>())
                {
                    var cmdNodeId = cmdType ?? string.Empty;
                    var toNode = nodeDict.ContainsKey(cmdNodeId) ? nodeDict[cmdNodeId] : null;
                    if (senderMethodNode != null && toNode != null)
                    {
                        relationships.Add(new Relationship
                        {
                            FromNode = senderMethodNode,
                            ToNode = toNode,
                            Type = RelationshipType.CommandSenderMethodToCommand
                        });
                    }
                }

                // CommandSenderToCommand 关系（Sender节点到Command节点）
                var senderNodeId = senderAttr.SenderType;
                var senderNode = nodeDict.ContainsKey(senderNodeId) ? nodeDict[senderNodeId] : null;
                foreach (var cmdType in senderAttr.CommandTypes ?? Array.Empty<string>())
                {
                    var cmdNodeId = cmdType ?? string.Empty;
                    var toNode = nodeDict.ContainsKey(cmdNodeId) ? nodeDict[cmdNodeId] : null;
                    if (senderNode != null && toNode != null)
                    {
                        relationships.Add(new Relationship
                        {
                            FromNode = senderNode,
                            ToNode = toNode,
                            Type = RelationshipType.CommandSenderToCommand
                        });
                    }
                }
            }
            else if (attr is CommandHandlerMetadataAttribute handlerAttr)
            {
                var fromNode = nodeDict.ContainsKey(handlerAttr.HandlerType) ? nodeDict[handlerAttr.HandlerType] : null;
                if (!string.IsNullOrEmpty(handlerAttr.CommandType) && !string.IsNullOrEmpty(handlerAttr.EntityType) &&
                    !string.IsNullOrEmpty(handlerAttr.EntityMethodName))
                {
                    var entityMethodNodeId = $"{handlerAttr.EntityType}.{handlerAttr.EntityMethodName}";
                    var toNode = nodeDict.ContainsKey(entityMethodNodeId) ? nodeDict[entityMethodNodeId] : null;
                    if (fromNode != null && toNode != null)
                    {
                        relationships.Add(new Relationship
                        {
                            FromNode = fromNode,
                            ToNode = toNode,
                            Type = RelationshipType.CommandToAggregateMethod
                        });
                    }
                }
            }
            else if (attr is DomainEventHandlerMetadataAttribute domainHandlerAttr)
            {
                var fromNode = nodeDict.ContainsKey(domainHandlerAttr.HandlerType)
                    ? nodeDict[domainHandlerAttr.HandlerType]
                    : null;
                var eventNode = nodeDict.ContainsKey(domainHandlerAttr.EventType)
                    ? nodeDict[domainHandlerAttr.EventType]
                    : null;
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
                            Type = RelationshipType.CommandSenderToCommand
                        });
                    }
                }
            }
            else if (attr is IntegrationEventHandlerMetadataAttribute integrationHandlerAttr)
            {
                var fromNode = nodeDict.ContainsKey(integrationHandlerAttr.HandlerType)
                    ? nodeDict[integrationHandlerAttr.HandlerType]
                    : null;
                var eventNode = nodeDict.ContainsKey(integrationHandlerAttr.EventType)
                    ? nodeDict[integrationHandlerAttr.EventType]
                    : null;
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
                            Type = RelationshipType.CommandSenderToCommand
                        });
                    }
                }
            }
            else if (attr is IntegrationEventConverterMetadataAttribute converterAttr)
            {
                var domainNode = nodeDict.ContainsKey(converterAttr.DomainEventType)
                    ? nodeDict[converterAttr.DomainEventType]
                    : null;
                var integrationNode = nodeDict.ContainsKey(converterAttr.IntegrationEventType)
                    ? nodeDict[converterAttr.IntegrationEventType]
                    : null;
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

    // 获取类名（去除命名空间）
    private static string GetClassName(string fullTypeName)
    {
        if (string.IsNullOrEmpty(fullTypeName)) return string.Empty;
        var lastDot = fullTypeName.LastIndexOf('.');
        return lastDot >= 0 ? fullTypeName.Substring(lastDot + 1) : fullTypeName;
    }
}