using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetCorePal.Extensions.CodeAnalysis;
using NetCorePal.Extensions.CodeAnalysis.Attributes;

namespace NetCorePal.Extensions.CodeAnalysis
{
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
            var aggregateMethodNodes = GetEntityMethodNodes(attributes);
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
            relationships.AddRange(
                GetControllerMethodToCommandRelationships(controllerMethodNodes, commandNodes, attributes));
            relationships.AddRange(GetEndpointToCommandRelationships(endpointNodes, commandNodes, attributes));
            relationships.AddRange(
                GetCommandSenderToCommandRelationships(commandSenderNodes, commandNodes, attributes));
            relationships.AddRange(
                GetCommandSenderMethodToCommandRelationships(commandSenderMethodNodes, commandNodes, attributes));
            relationships.AddRange(GetCommandToAggregateRelationships(commandNodes, aggregateNodes, attributes));
            relationships.AddRange(
                GetCommandToEntityMethodRelationships(commandNodes, aggregateMethodNodes, attributes));
            relationships.AddRange(
                GetAggregateToDomainEventRelationships(aggregateNodes, domainEventNodes, attributes));
            relationships.AddRange(GetEntityMethodToEntityMethodRelationships(aggregateMethodNodes,
                aggregateMethodNodes, attributes));
            relationships.AddRange(
                GetEntityMethodToDomainEventRelationships(aggregateMethodNodes, domainEventNodes, attributes));
            relationships.AddRange(
                GetDomainEventToHandlerRelationships(domainEventNodes, domainEventHandlerNodes, attributes));
            relationships.AddRange(
                GetDomainEventHandlerToCommandRelationships(domainEventHandlerNodes, commandNodes, attributes));
            relationships.AddRange(
                GetIntegrationEventToHandlerRelationships(integrationEventNodes, integrationEventHandlerNodes,
                    attributes));
            relationships.AddRange(GetIntegrationEventHandlerToCommandRelationships(integrationEventHandlerNodes,
                commandNodes, attributes));
            relationships.AddRange(
                GetDomainEventToIntegrationEventRelationships(domainEventNodes, integrationEventNodes, attributes));

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
            => attributes.OfType<ControllerMethodMetadataAttribute>()
                .Where(attr => attr.CommandTypes != null && attr.CommandTypes.Length > 0)
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
            => attributes.OfType<ControllerMethodMetadataAttribute>()
                .Where(attr => attr.CommandTypes != null && attr.CommandTypes.Length > 0)
                .Select(attr => new Node
                {
                    Id = $"{attr.ControllerType}.{attr.ControllerMethodName}",
                    Name = $"{GetClassName(attr.ControllerType)}.{attr.ControllerMethodName}",
                    FullName = $"{attr.ControllerType}.{attr.ControllerMethodName}",
                    Type = NodeType.ControllerMethod
                }).ToList();

        public static List<Node> GetEndpointNodes(IEnumerable<MetadataAttribute> attributes)
            => attributes.OfType<EndpointMetadataAttribute>()
                .Where(attr => attr.CommandTypes != null && attr.CommandTypes.Length > 0)
                .Select(attr => new Node
                {
                    Id = $"{attr.EndpointType}.{attr.EndpointMethodName}",
                    Name = GetClassName(attr.EndpointType),
                    FullName = $"{attr.EndpointType}.{attr.EndpointMethodName}",
                    Type = NodeType.Endpoint
                }).ToList();


        public static List<Node> GetCommandSenderNodes(IEnumerable<MetadataAttribute> attributes)
            => attributes.OfType<CommandSenderMethodMetadataAttribute>()
                .Where(attr => attr.CommandTypes != null && attr.CommandTypes.Length > 0)
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
            => attributes.OfType<CommandSenderMethodMetadataAttribute>()
                .Where(attr => attr.CommandTypes != null && attr.CommandTypes.Length > 0)
                .Select(attr => new Node
                {
                    Id = $"{attr.SenderType}.{attr.SenderMethodName}",
                    Name = $"{GetClassName(attr.SenderType)}.{attr.SenderMethodName}",
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

        public static List<Node> GetEntityMethodNodes(IEnumerable<MetadataAttribute> attributes)
            => attributes.OfType<EntityMethodMetadataAttribute>().Select(attr => new Node
            {
                Id = $"{attr.EntityType}.{attr.MethodName}",
                Name = $"{GetClassName(attr.EntityType)}.{attr.MethodName}",
                FullName = $"{attr.EntityType}.{attr.MethodName}",
                Type = NodeType.EntityMethod
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
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.Aggregate && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());

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
            // 只生成 ControllerMethodMetadataAttribute 中实际存在的 Controller-Command 关系
            var controllerAttrs = attributes.OfType<ControllerMethodMetadataAttribute>();
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.Controller && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.Command && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());

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
            IEnumerable<Node> toNodes,
            IEnumerable<MetadataAttribute> attributes)
        {
            // 只生成 ControllerMethodMetadataAttribute 中实际存在的 ControllerMethod-Command 关系
            var controllerMethodAttrs = attributes.OfType<ControllerMethodMetadataAttribute>();
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.ControllerMethod && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.Command && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());

            var relationships = new List<Relationship>();
            foreach (var attr in controllerMethodAttrs)
            {
                var methodId = $"{attr.ControllerType}.{attr.ControllerMethodName}";
                if (fromNodeDict.TryGetValue(methodId, out var fromNode) && attr.CommandTypes != null)
                {
                    foreach (var cmdType in attr.CommandTypes)
                    {
                        if (toNodeDict.TryGetValue(cmdType, out var toNode))
                        {
                            relationships.Add(new Relationship(fromNode, toNode,
                                RelationshipType.ControllerMethodToCommand));
                        }
                    }
                }
            }

            return relationships;
        }

        public static List<Relationship> GetEndpointToCommandRelationships(IEnumerable<Node> fromNodes,
            IEnumerable<Node> toNodes, IEnumerable<MetadataAttribute> attributes)
        {
            // 只生成 EndpointMetadataAttribute 中实际存在的 Endpoint-Command 关系
            var endpointAttrs = attributes.OfType<EndpointMetadataAttribute>();
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.Endpoint && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.Command && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());

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
            IEnumerable<Node> toNodes,
            IEnumerable<MetadataAttribute> attributes)
        {
            // 只生成 CommandSenderMethodMetadataAttribute 中实际存在的 CommandSender-Command 关系
            var senderMethodAttrs = attributes.OfType<CommandSenderMethodMetadataAttribute>();
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.CommandSender && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.Command && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());

            var relationships = new List<Relationship>();
            foreach (var attr in senderMethodAttrs)
            {
                if (fromNodeDict.TryGetValue(attr.SenderType, out var fromNode) && attr.CommandTypes != null)
                {
                    foreach (var cmdType in attr.CommandTypes)
                    {
                        if (toNodeDict.TryGetValue(cmdType, out var toNode))
                        {
                            relationships.Add(new Relationship(fromNode, toNode,
                                RelationshipType.CommandSenderToCommand));
                        }
                    }
                }
            }

            return relationships;
        }

        public static List<Relationship> GetCommandSenderMethodToCommandRelationships(IEnumerable<Node> fromNodes,
            IEnumerable<Node> toNodes,
            IEnumerable<MetadataAttribute> attributes)
        {
            // 只生成 CommandSenderMethodMetadataAttribute 中实际存在的 CommandSenderMethod-Command 关系
            var senderMethodAttrs = attributes.OfType<CommandSenderMethodMetadataAttribute>();
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.CommandSenderMethod && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.Command && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());

            var relationships = new List<Relationship>();
            foreach (var attr in senderMethodAttrs)
            {
                var methodId = $"{attr.SenderType}.{attr.SenderMethodName}";
                if (fromNodeDict.TryGetValue(methodId, out var fromNode) && attr.CommandTypes != null)
                {
                    foreach (var cmdType in attr.CommandTypes)
                    {
                        if (toNodeDict.TryGetValue(cmdType, out var toNode))
                        {
                            relationships.Add(new Relationship(fromNode, toNode,
                                RelationshipType.CommandSenderMethodToCommand));
                        }
                    }
                }
            }

            return relationships;
        }

        public static List<Relationship> GetCommandToEntityMethodRelationships(IEnumerable<Node> fromNodes,
            IEnumerable<Node> toNodes,
            IEnumerable<MetadataAttribute> attributes)
        {
            // 只生成 CommandHandlerEntityMethodMetadataAttribute 中实际存在的 Command-EntityMethod 关系
            var handlerEntityMethodAttrs = attributes.OfType<CommandHandlerEntityMethodMetadataAttribute>();
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.Command && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.EntityMethod && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());

            var relationships = new List<Relationship>();
            foreach (var attr in handlerEntityMethodAttrs)
            {
                var methodFullName = $"{attr.EntityType}.{attr.EntityMethodName}";
                if (fromNodeDict.TryGetValue(attr.CommandType, out var fromNode) &&
                    toNodeDict.TryGetValue(methodFullName, out var toNode))
                {
                    relationships.Add(new Relationship(fromNode, toNode, RelationshipType.CommandToEntityMethod));
                }
            }

            return relationships;
        }

        /// <summary>
        /// 获取聚合根到领域事件的关系（通过 EntityMethodMetadataAttribute 分析）
        /// </summary>
        /// <param name="fromNodes"></param>
        /// <param name="toNodes"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static List<Relationship> GetAggregateToDomainEventRelationships(IEnumerable<Node> fromNodes,
            IEnumerable<Node> toNodes, IEnumerable<MetadataAttribute> attributes)
        {
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.Aggregate && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.DomainEvent && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());

            var relationships = new List<Relationship>();
            foreach (var aggregateNode in fromNodeDict)
            {
                var eventTypes = GetAggregateDomanEventTypes(attributes, aggregateNode.Key, new List<string>());
                foreach (var eventType in eventTypes)
                {
                    if (toNodeDict.TryGetValue(eventType, out var toNode))
                    {
                        relationships.Add(new Relationship(aggregateNode.Value, toNode,
                            RelationshipType.AggregateToDomainEvent));
                    }
                }
            }

            // 去重
            var uniqueRelationships = relationships
                .GroupBy(r => (r.FromNode.Id, r.ToNode.Id, r.Type))
                .Select(g => g.First())
                .ToList();
            return uniqueRelationships;
        }

        /// <summary>
        /// 获取聚合方法到聚合方法的关系（通过 EntityMethodMetadataAttribute 分析）
        /// </summary>
        /// <param name="fromNodes">聚合方法节点</param>
        /// <param name="toNodes">聚合方法节点</param>
        /// <param name="attributes">所有元数据属性</param>
        /// <returns></returns>
        public static List<Relationship> GetEntityMethodToEntityMethodRelationships(
            IEnumerable<Node> fromNodes,
            IEnumerable<Node> toNodes,
            IEnumerable<MetadataAttribute> attributes)
        {
            // 只生成 EntityMethodMetadataAttribute 中实际存在的 EntityMethod->EntityMethod 关系
            var entityMethodAttrs = attributes.OfType<EntityMethodMetadataAttribute>();
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.EntityMethod && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.EntityMethod && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());

            var relationships = new List<Relationship>();
            foreach (var attr in entityMethodAttrs)
            {
                var methodId = $"{attr.EntityType}.{attr.MethodName}";
                if (fromNodeDict.TryGetValue(methodId, out var fromNode) && attr.CalledEntityMethods != null)
                {
                    foreach (var calledMethodFullName in attr.CalledEntityMethods)
                    {
                        if (toNodeDict.TryGetValue(calledMethodFullName, out var toNode))
                        {
                            relationships.Add(new Relationship(fromNode, toNode,
                                RelationshipType.EntityMethodToEntityMethod));
                        }
                    }
                }
            }

            return relationships;
        }


        public static List<Relationship> GetEntityMethodToDomainEventRelationships(IEnumerable<Node> fromNodes,
            IEnumerable<Node> toNodes,
            IEnumerable<MetadataAttribute> attributes)
        {
            // 只生成 EntityMethodMetadataAttribute 中实际存在的 EntityMethod-DomainEvent 关系
            var entityMethodAttrs = attributes.OfType<EntityMethodMetadataAttribute>();
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.EntityMethod && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.DomainEvent && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());

            var relationships = new List<Relationship>();
            foreach (var attr in entityMethodAttrs)
            {
                var methodId = $"{attr.EntityType}.{attr.MethodName}";
                if (fromNodeDict.TryGetValue(methodId, out var fromNode) && attr.EventTypes != null)
                {
                    foreach (var eventType in attr.EventTypes)
                    {
                        if (toNodeDict.TryGetValue(eventType, out var toNode))
                        {
                            relationships.Add(new Relationship(fromNode, toNode,
                                RelationshipType.EntityMethodToDomainEvent));
                        }
                    }
                }
            }

            return relationships;
        }

        public static List<Relationship> GetDomainEventToHandlerRelationships(
            IEnumerable<Node> fromNodes,
            IEnumerable<Node> toNodes,
            IEnumerable<MetadataAttribute> attributes)
        {
            var relationships = new List<Relationship>();
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.DomainEvent && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.DomainEventHandler && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var handlerMetas = attributes.OfType<DomainEventHandlerMetadataAttribute>().ToList();
            foreach (var handler in handlerMetas)
            {
                if (fromNodeDict.TryGetValue(handler.EventType, out var fromNode) &&
                    toNodeDict.TryGetValue(handler.HandlerType, out var toNode))
                {
                    relationships.Add(new Relationship(fromNode, toNode, RelationshipType.DomainEventToHandler));
                }
            }

            return relationships;
        }

        /// <summary>
        /// 获取领域事件处理器到命令的关系（通过 DomainEventHandlerMetadataAttribute 分析）
        /// </summary>
        /// <param name="fromNodes">领域事件处理器节点</param>
        /// <param name="toNodes">命令节点</param>
        /// <param name="attributes">所有元数据属性</param>
        /// <returns></returns>
        public static List<Relationship> GetDomainEventHandlerToCommandRelationships(
            IEnumerable<Node> fromNodes,
            IEnumerable<Node> toNodes,
            IEnumerable<MetadataAttribute> attributes)
        {
            var relationships = new List<Relationship>();
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.DomainEventHandler && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.Command && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var handlerMetas = attributes.OfType<DomainEventHandlerMetadataAttribute>().ToList();
            foreach (var handler in handlerMetas)
            {
                if (fromNodeDict.TryGetValue(handler.HandlerType, out var fromNode) &&
                    handler.CommandTypes != null)
                {
                    foreach (var cmdType in handler.CommandTypes)
                    {
                        if (toNodeDict.TryGetValue(cmdType, out var toNode))
                        {
                            relationships.Add(new Relationship(fromNode, toNode,
                                RelationshipType.DomainEventHandlerToCommand));
                        }
                    }
                }
            }

            return relationships;
        }

        public static List<Relationship> GetIntegrationEventToHandlerRelationships(IEnumerable<Node> fromNodes,
            IEnumerable<Node> toNodes,
            IEnumerable<MetadataAttribute> attributes)
        {
            var relationships = new List<Relationship>();
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.IntegrationEvent && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.IntegrationEventHandler && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var handlerMetas = attributes.OfType<IntegrationEventHandlerMetadataAttribute>().ToList();
            foreach (var handler in handlerMetas)
            {
                if (fromNodeDict.TryGetValue(handler.EventType, out var fromNode) &&
                    toNodeDict.TryGetValue(handler.HandlerType, out var toNode))
                {
                    relationships.Add(new Relationship(fromNode, toNode, RelationshipType.IntegrationEventToHandler));
                }
            }

            return relationships;
        }

        /// <summary>
        /// 获取集成事件处理器到命令的关系（通过 IntegrationEventHandlerMetadataAttribute 分析）
        /// </summary>
        /// <param name="fromNodes">集成事件处理器节点</param>
        /// <param name="toNodes">命令节点</param>
        /// <param name="attributes">所有元数据属性</param>
        /// <returns></returns>
        public static List<Relationship> GetIntegrationEventHandlerToCommandRelationships(
            IEnumerable<Node> fromNodes,
            IEnumerable<Node> toNodes,
            IEnumerable<MetadataAttribute> attributes)
        {
            var relationships = new List<Relationship>();
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.IntegrationEventHandler && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.Command && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var handlerMetas = attributes.OfType<IntegrationEventHandlerMetadataAttribute>().ToList();
            foreach (var handler in handlerMetas)
            {
                if (fromNodeDict.TryGetValue(handler.HandlerType, out var fromNode) &&
                    handler.CommandTypes != null)
                {
                    foreach (var cmdType in handler.CommandTypes)
                    {
                        if (toNodeDict.TryGetValue(cmdType, out var toNode))
                        {
                            relationships.Add(new Relationship(fromNode, toNode,
                                RelationshipType.IntegrationEventHandlerToCommand));
                        }
                    }
                }
            }

            return relationships;
        }

        public static List<Relationship> GetDomainEventToIntegrationEventRelationships(
            IEnumerable<Node> fromNodes,
            IEnumerable<Node> toNodes,
            IEnumerable<MetadataAttribute> attributes)
        {
            var relationships = new List<Relationship>();
            var fromNodeDict = fromNodes.Where(n => n.Type == NodeType.DomainEvent && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var toNodeDict = toNodes.Where(n => n.Type == NodeType.IntegrationEvent && n.Id != null)
                .GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
            var converterMetas = attributes.OfType<IntegrationEventConverterMetadataAttribute>().ToList();
            foreach (var converter in converterMetas)
            {
                if (fromNodeDict.TryGetValue(converter.DomainEventType, out var fromNode) &&
                    toNodeDict.TryGetValue(converter.IntegrationEventType, out var toNode))
                {
                    relationships.Add(
                        new Relationship(fromNode, toNode, RelationshipType.DomainEventToIntegrationEvent));
                }
            }

            return relationships;
        }

        #endregion


        #region 私有方法

        private static string GetClassName(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName)) return string.Empty;
            var lastDot = fullTypeName.LastIndexOf('.');
            return lastDot >= 0 ? fullTypeName.Substring(lastDot + 1) : fullTypeName;
        }


        private static List<string> GetAggregateDomanEventTypes(
            IEnumerable<MetadataAttribute> attributes, string aggregateType, List<string> entityTypes)
        {
            var eventTypes = new List<string>();
            EntityMetadataAttribute? aggregateAttr = attributes
                .OfType<EntityMetadataAttribute>()
                .FirstOrDefault(attr => attr.EntityType == aggregateType);
            if (aggregateAttr == null) return eventTypes;

            if (entityTypes.Contains(aggregateType))
            {
                return eventTypes; // 如果已经处理过该聚合根，直接返回空列表
            }

            entityTypes.Add(aggregateType);


            var entityMethodMetadataAttributes = attributes.OfType<EntityMethodMetadataAttribute>()
                .Where(attr => attr.EntityType == aggregateType).ToList();
            foreach (var entityMethodMetadataAttribute in entityMethodMetadataAttributes)
            {
                if (entityMethodMetadataAttribute.EventTypes != null)
                {
                    eventTypes.AddRange(entityMethodMetadataAttribute.EventTypes);
                }
            }

            if (aggregateAttr.SubEntities != null && aggregateAttr.SubEntities.Any())
            {
                // 如果有子实体，递归获取子实体的领域事件类型
                foreach (var subEntity in aggregateAttr.SubEntities)
                {
                    eventTypes.AddRange(GetAggregateDomanEventTypes(attributes, subEntity, new List<string>(entityTypes)));
                }
            }

            return eventTypes.Distinct().ToList();
        }

        #endregion
    }
}