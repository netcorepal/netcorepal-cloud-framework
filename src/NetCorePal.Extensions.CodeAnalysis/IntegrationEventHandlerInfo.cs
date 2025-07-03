using System.Collections.Generic;

namespace NetCorePal.Extensions.CodeAnalysis;

public class IntegrationEventHandlerInfo
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string HandledEventType { get; set; } = string.Empty;
    
    /// <summary>
    /// 发出的命令列表
    /// </summary>
    public List<string> Commands { get; set; } = new();
}

