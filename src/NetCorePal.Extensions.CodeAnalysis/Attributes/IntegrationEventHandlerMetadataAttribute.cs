namespace NetCorePal.Extensions.CodeAnalysis.Attributes;
using System;

/// <summary>
/// 集成事件处理器元数据特性，用于标识集成事件处理器类型、事件类型及其发出的命令类型。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class IntegrationEventHandlerMetadataAttribute : MetadataAttribute
{
    /// <summary>
    /// 集成事件处理器类型
    /// </summary>
    public string HandlerType { get; }
    
    /// <summary>
    /// 集成事件类型
    /// </summary>
    public string EventType { get; }
    
    /// <summary>
    /// 集成事件发出的命令类型列表
    /// </summary>
    public string[] CommandTypes { get; }

    /// <summary>
    /// 构造函数，初始化集成事件处理器元数据特性。
    /// </summary>
    /// <param name="handlerType">集成事件处理器类型</param>
    /// <param name="eventType">集成事件类型</param>
    /// <param name="commandTypes">集成事件发出的命令类型列表</param>
    public IntegrationEventHandlerMetadataAttribute(string handlerType,string eventType, params string[] commandTypes)
    {
        HandlerType = handlerType;
        EventType = eventType;
        CommandTypes = commandTypes;
    }
}
