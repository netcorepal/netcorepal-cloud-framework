namespace NetCorePal.Extensions.CodeAnalysis.Attributes;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class CommandHandlerMetadataAttribute : MetadataAttribute
{
    /// <summary>
    /// 命令处理器的类型。
    /// </summary>
    public string HandlerType { get; }

    /// <summary>
    /// 命令类型。
    /// </summary>
    public string CommandType { get; }

    /// <summary>
    /// 聚合类型列表。
    /// </summary>
    public string[] AggregateTypes { get; }

    /// <summary>
    /// 构造函数，初始化命令处理器元数据特性。
    /// </summary>
    /// <param name="handlerType">命令处理器类型</param>
    /// <param name="commandType">命令类型</param>
    /// <param name="aggregateTypes">相关的聚合列表</param>
    public CommandHandlerMetadataAttribute(string handlerType, string commandType, params string[] aggregateTypes)
    {
        HandlerType = handlerType;
        CommandType = commandType;
        AggregateTypes = aggregateTypes;
    }
}