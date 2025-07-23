namespace NetCorePal.Extensions.CodeAnalysis.Attributes;

using System;

/// <summary>
/// Endpoint元数据特性，用于标识Endpoint类型、方法名称及其处理的命令类型。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class EndpointMetadataAttribute : Attribute
{
    /// <summary>
    /// Endpoint的类型。
    /// </summary>
    public string EndpointType { get; }

    /// <summary>
    /// Endpoint的方法名称。
    /// </summary>
    public string EndpointMethodName { get; }

    /// <summary>
    /// 该方法发出的命令列表
    /// </summary>
    public string[] CommandTypes { get; }

    /// <summary>
    /// 构造函数，初始化Controller元数据特性。
    /// </summary>
    /// <param name="endpointType">Endpoint的类型。</param>
    /// <param name="endpointMethodName">Endpoint的方法名称。</param>
    /// <param name="commandTypes">该方法发出的命令列表</param>
    public EndpointMetadataAttribute(string endpointType, string endpointMethodName, params string[] commandTypes)
    {
        EndpointType = endpointType;
        EndpointMethodName = endpointMethodName;
        CommandTypes = commandTypes;
    }
}