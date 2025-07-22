namespace NetCorePal.Extensions.CodeAnalysis.Attributes;

using System;

/// <summary>
/// 用于描述实体、子实体属性以及方法清单的元数据Attribute。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class EntityMetadataAttribute : Attribute
{
    /// <summary>
    /// 实体类型
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// 是否是聚合根
    /// </summary>
    public bool IsAggregateRoot { get; }

    /// <summary>
    /// 实体属性包含的子实体类型列表
    /// </summary>
    public string[] SubEntities { get; }

    /// <summary>
    /// 实体自己的方法清单，同名方法会被覆盖。
    /// </summary>
    public string[] MethodNames { get; }

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="entityType">实体类型全名</param>
    /// <param name="isAggregateRoot">是否为聚合根</param>
    /// <param name="subEntities">子实体名清单</param>
    /// <param name="methodNames">实体自己的方法清单，同名方法会被覆盖。</param>
    public EntityMetadataAttribute(string entityType, bool isAggregateRoot, string[] subEntities, string[] methodNames)
    {
        EntityType = entityType;
        IsAggregateRoot = isAggregateRoot;
        SubEntities = subEntities;
        MethodNames = methodNames;
    }
}