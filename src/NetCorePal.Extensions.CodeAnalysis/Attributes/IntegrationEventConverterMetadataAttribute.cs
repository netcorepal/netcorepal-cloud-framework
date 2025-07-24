namespace NetCorePal.Extensions.CodeAnalysis.Attributes;

using System;

/// <summary>
/// 集成事件转换器元数据特性，用于标识领域事件与集成事件之间的转换关系。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class IntegrationEventConverterMetadataAttribute : MetadataAttribute
{
    /// <summary>
    /// 领域事件的类型。
    /// </summary>
    public string DomainEventType { get; }

    /// <summary>
    /// 集成事件的类型。
    /// </summary>
    public string IntegrationEventType { get; }

    /// <summary>
    /// 构造函数，初始化集成事件转换器元数据特性。
    /// </summary>
    /// <param name="domainEventType">领域事件的类型</param>
    /// <param name="integrationEventType">领域事件的类型</param>
    public IntegrationEventConverterMetadataAttribute(string domainEventType, string integrationEventType)
    {
        DomainEventType = domainEventType;
        IntegrationEventType = integrationEventType;
    }
}