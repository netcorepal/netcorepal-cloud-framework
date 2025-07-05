namespace NetCorePal.Extensions.CodeAnalysis;

/// <summary>
/// 调用关系
/// </summary>
public class CallRelationship
{
    /// <summary>
    /// 源类型
    /// </summary>
    public string SourceType { get; set; } = "";

    /// <summary>
    /// 源方法
    /// </summary>
    public string SourceMethod { get; set; } = "";

    /// <summary>
    /// 目标类型
    /// </summary>
    public string TargetType { get; set; } = "";

    /// <summary>
    /// 目标方法
    /// </summary>
    public string TargetMethod { get; set; } = "";

    /// <summary>
    /// 调用类型
    /// </summary>
    public string CallType { get; set; } = string.Empty;

    public CallRelationship(string sourceType, string sourceMethod, string targetType, string targetMethod, string callType)
    {
        SourceType = sourceType;
        SourceMethod = sourceMethod;
        TargetType = targetType;
        TargetMethod = targetMethod;
        CallType = callType;
    }
}
