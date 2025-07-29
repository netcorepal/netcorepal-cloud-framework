namespace NetCorePal.Extensions.CodeAnalysis.Attributes;

using System;

/// <summary>
/// 命令发起者元数据特性，用于标识命令发起者类型及其方法。（不包含Controller、Endpoint、DomainEventHandler、IntegrationEventHandler）
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class CommandSenderMethodMetadataAttribute : MetadataAttribute
{
    /// <summary>
    /// 发起命令者的类型
    /// </summary>
    public string SenderType { get; }

    /// <summary>
    /// 发起命令者的方法名称
    /// </summary>
    public string SenderMethodName { get; }

    /// <summary>
    /// 发出的命令类型列表
    /// </summary>
    public string[] CommandTypes { get; }

    /// <summary>
    /// 构造函数，初始化命令发起者元数据特性。
    /// </summary>
    /// <param name="senderType">发起命令者的类型</param>
    /// <param name="senderMethodName">发起命令者的方法名称</param>
    /// <param name="commandTypes">发出的命令类型列表</param>
    public CommandSenderMethodMetadataAttribute(string senderType, string senderMethodName, params string[] commandTypes)
    {
        SenderType = senderType;
        SenderMethodName = senderMethodName;
        CommandTypes = commandTypes;
    }
}