namespace NetCorePal.Extensions.Domain;

/// <summary>
///     表示一个数据的删除时间。
/// </summary>
/// <param name="Value">记录的已删除时间，使用 <see cref="DateTimeOffset" /> 类型</param>
public record DeletedTime(DateTimeOffset Value);