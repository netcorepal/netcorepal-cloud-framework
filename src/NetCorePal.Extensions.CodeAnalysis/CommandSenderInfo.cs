using System.Collections.Generic;

namespace NetCorePal.Extensions.CodeAnalysis;

/// <summary>
/// 命令发送者信息
/// </summary>
public class CommandSenderInfo
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
    /// 发送命令的方法列表
    /// </summary>
    public List<string> Methods { get; set; } = new();
}
