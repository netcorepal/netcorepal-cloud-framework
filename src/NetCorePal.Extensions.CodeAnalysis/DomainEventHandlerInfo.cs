using System.Collections.Generic;

namespace NetCorePal.Extensions.CodeAnalysis;

/// <summary>
/// 领域事件处理器信息
/// </summary>
public class DomainEventHandlerInfo
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
    /// 处理的事件类型
    /// </summary>
    public string HandledEventType { get; set; } = "";

    /// <summary>
    /// 发出的命令列表
    /// </summary>
    public List<string> Commands { get; set; } = new();
}

