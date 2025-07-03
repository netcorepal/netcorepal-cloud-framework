using System.Collections.Generic;

namespace NetCorePal.Extensions.CodeAnalysis;

/// <summary>
/// 领域事件信息
/// </summary>
public class DomainEventInfo
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 完整名称
    /// </summary>
    public string FullName { get; set; } = "";

    /// <summary>
    /// 属性列表
    /// </summary>
    public List<string> Properties { get; set; } = new();
}

