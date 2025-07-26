namespace NetCorePal.Extensions.CodeAnalysis.Attributes;

using System;

/// <summary>
/// 命令元数据特性，用于标识命令类型。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class CommandMetadataAttribute : MetadataAttribute
{
    /// <summary>
    /// 命令类型。
    /// </summary>
    public string CommandType { get; }

    /// <summary>
    /// 构造函数，初始化命令元数据特性。
    /// </summary>
    /// <param name="commandType">命令类型</param>
    public CommandMetadataAttribute(string commandType)
    {
        CommandType = commandType;
    }
}