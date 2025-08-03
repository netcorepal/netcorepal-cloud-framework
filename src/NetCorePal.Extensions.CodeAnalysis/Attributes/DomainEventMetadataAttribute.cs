namespace NetCorePal.Extensions.CodeAnalysis.Attributes;

using System;

/// <summary>
/// 领域事件元数据特性，用于标识领域事件类型。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DomainEventMetadataAttribute : MetadataAttribute
{
    /// <summary>
    /// 领域事件的类型。
    /// </summary>
    public string EventType { get; }

    /// <summary>
    /// 构造函数，初始化领域事件元数据特性。
    /// </summary>
    /// <param name="eventType">领域事件的类型</param>
    public DomainEventMetadataAttribute(string eventType)
    {
        EventType = eventType;
    }
}