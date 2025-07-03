using System.Collections.Generic;

namespace NetCorePal.Extensions.CodeAnalysis;

/// <summary>
/// 实体信息
/// </summary>
public class EntityInfo
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
    /// 是否为聚合根
    /// </summary>
    public bool IsAggregateRoot { get; set; }

    /// <summary>
    /// 方法列表
    /// </summary>
    public List<string> Methods { get; set; } = new();
}

