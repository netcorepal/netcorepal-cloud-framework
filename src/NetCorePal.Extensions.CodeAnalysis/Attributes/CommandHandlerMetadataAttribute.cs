namespace NetCorePal.Extensions.CodeAnalysis.Attributes;

using System;

/// <summary>
/// 标识命令处理器的元数据特性。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class CommandHandlerMetadataAttribute : MetadataAttribute
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
    /// 命令调用的实体的类型。
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// 命令调用的实体的方法
    /// </summary>
    public string EntityMethodName { get; }

    /// <summary>
    /// 构造函数，初始化命令处理器元数据特性。
    /// </summary>
    /// <param name="handlerType">命令处理器的类型</param>
    /// <param name="commandType">命令类型</param>
    /// <param name="entityType">实体类型</param>
    /// <param name="entityMethodName">命令调用的实体的方法</param>
    public CommandHandlerMetadataAttribute(string handlerType, string commandType, string entityType,
        string entityMethodName)
    {
        HandlerType = handlerType;
        CommandType = commandType;
        EntityType = entityType;
        EntityMethodName = entityMethodName;
    }
}