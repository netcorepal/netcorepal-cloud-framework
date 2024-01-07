using System.Text.Json;

namespace NetCorePal.SkyApm.Diagnostics;

public class NetCorePalTracingOptions
{
    /// <summary>
    /// 是否记录Command的详细数据，将使用System.Text.Json序列化命令数据
    /// </summary>
    public bool WriteCommandData { get; set; } = false;

    /// <summary>
    /// 是否记录的DomainEvent详细数据，将使用System.Text.Json序列化命令数据
    /// </summary>
    public bool WriteDomainEventData { get; set; } = false;

    /// <summary>
    /// 序列化命令数据时的JsonSerializerOptions选项
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions()
    {
        WriteIndented = false
    };
}