using System.Collections.Generic;

namespace NetCorePal.Extensions.CodeAnalysis;

/// <summary>
/// 代码流分析结果
/// </summary>
public class CodeFlowAnalysisResult
{
    /// <summary>
    /// 控制器列表
    /// </summary>
    public List<ControllerInfo> Controllers { get; set; } = new();

    /// <summary>
    /// 命令列表
    /// </summary>
    public List<CommandInfo> Commands { get; set; } = new();

    /// <summary>
    /// 实体列表
    /// </summary>
    public List<EntityInfo> Entities { get; set; } = new();

    /// <summary>
    /// 领域事件列表
    /// </summary>
    public List<DomainEventInfo> DomainEvents { get; set; } = new();

    /// <summary>
    /// 领域事件处理器列表
    /// </summary>
    public List<DomainEventHandlerInfo> DomainEventHandlers { get; set; } = new();

    /// <summary>
    /// 集成事件列表
    /// </summary>
    public List<IntegrationEventInfo> IntegrationEvents { get; set; } = new();

    /// <summary>
    /// 集成事件处理器列表
    /// </summary>
    public List<IntegrationEventHandlerInfo> IntegrationEventHandlers { get; set; } = new();

    /// <summary>
    /// 集成事件转换器列表
    /// </summary>
    public List<IntegrationEventConverterInfo> IntegrationEventConverters { get; set; } = new();

    /// <summary>
    /// 调用关系列表
    /// </summary>
    public List<CallRelationship> Relationships { get; set; } = new();
}

