namespace NetCorePal.Extensions.CodeAnalysis;

/// <summary>
/// 集成事件转换器信息
/// </summary>
public class IntegrationEventConverterInfo
{
    /// <summary>
    /// 转换器名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 转换器完整名称
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// 源领域事件类型
    /// </summary>
    public string DomainEventType { get; set; } = string.Empty;
    
    /// <summary>
    /// 目标集成事件类型
    /// </summary>
    public string IntegrationEventType { get; set; } = string.Empty;
}
