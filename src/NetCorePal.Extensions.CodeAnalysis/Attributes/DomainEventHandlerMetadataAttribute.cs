namespace NetCorePal.Extensions.CodeAnalysis.Attributes;

using System;

/// <summary>
/// 领域事件处理器元数据特性，用于标识领域事件处理器类型、事件类型及其发出的命令类型。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DomainEventHandlerMetadataAttribute : Attribute
{
    /// <summary>
    /// 领域事件处理器的类型。
    /// </summary>
    public string HandlerType { get; }

    /// <summary>
    /// 领域事件的类型。
    /// </summary>
    public string EventType { get; }

    /// <summary>
    /// 处理器发出的命令类型列表。
    /// </summary>
    public string[] CommandTypes { get; }

    /// <summary>
    /// 构造函数，初始化领域事件处理器元数据特性。
    /// </summary>
    /// <param name="handlerType">领域事件处理器的类型</param>
    /// <param name="eventType">领域事件的类型</param>
    /// <param name="commandTypes">处理器发出的命令类型列表</param>
    public DomainEventHandlerMetadataAttribute(string handlerType, string eventType, params string[] commandTypes)
    {
        HandlerType = handlerType;
        EventType = eventType;
        CommandTypes = commandTypes;
    }
}