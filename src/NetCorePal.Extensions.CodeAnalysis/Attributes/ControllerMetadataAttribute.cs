namespace NetCorePal.Extensions.CodeAnalysis.Attributes;

using System;

/// <summary>
/// Controller元数据特性，用于标识控制器类型、方法名称及其处理的命令类型。包括Controller、Endpoint。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ControllerMetadataAttribute : Attribute
{
    /// <summary>
    /// Controller的类型
    /// </summary>
    public string ControllerType { get; }

    /// <summary>
    /// Controller的方法名
    /// </summary>
    public string ControllerMethodName { get; }

    /// <summary>
    /// 该方法发出的命令列表
    /// </summary>
    public string[] CommandTypes { get; }

    /// <summary>
    /// 构造函数，初始化Controller元数据特性。
    /// </summary>
    /// <param name="controllerType">Controller的类型。</param>
    /// <param name="controllerMethodName"> Controller的方法名称。</param>
    /// <param name="commandTypes">该方法发出的命令列表</param>
    public ControllerMetadataAttribute(string controllerType, string controllerMethodName, params string[] commandTypes)
    {
        ControllerType = controllerType;
        ControllerMethodName = controllerMethodName;
        CommandTypes = commandTypes;
    }
}