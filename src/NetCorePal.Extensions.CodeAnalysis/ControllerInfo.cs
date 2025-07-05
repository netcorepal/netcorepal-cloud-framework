using System.Collections.Generic;

namespace NetCorePal.Extensions.CodeAnalysis;

/// <summary>
/// 控制器信息
/// </summary>
public class ControllerInfo
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
    /// 方法列表
    /// </summary>
    public List<string> Methods { get; set; } = new();
}

