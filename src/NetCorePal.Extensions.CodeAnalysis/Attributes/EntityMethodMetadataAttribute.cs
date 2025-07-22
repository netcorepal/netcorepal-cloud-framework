namespace NetCorePal.Extensions.CodeAnalysis.Attributes;

using System;

/// <summary>
/// 实体方法元数据特性，用于标识实体类型、方法名称、事件类型和调用的实体方法。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class EntityMethodMetadataAttribute : Attribute
{
    /// <summary>
    /// 实体类型
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// 实体方法名称，同名方法会被覆盖。
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// 该方法发出的事件类型列表。
    /// </summary>
    public string[] EventTypes { get; }

    /// <summary>
    /// 该方法调用的其它实体的方法列表。
    /// </summary>
    public string[] CalledEntityMethods { get; }

    /// <summary>
    /// 构造函数，初始化实体方法元数据特性。
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="methodName">实体方法名称，同名方法会被覆盖。</param>
    /// <param name="eventTypes">该方法发出的事件类型列表。</param>
    /// <param name="calledEntityMethods">该方法调用的其它实体的方法列表。</param>
    public EntityMethodMetadataAttribute(string entityType, string methodName, string[] eventTypes,
        string[] calledEntityMethods)
    {
        EntityType = entityType;
        MethodName = methodName;
        EventTypes = eventTypes;
        CalledEntityMethods = calledEntityMethods;
    }
}