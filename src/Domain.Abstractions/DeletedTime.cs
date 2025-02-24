namespace NetCorePal.Extensions.Domain;

/// <summary>
///     表示一个数据的删除时间。需与 <see cref="Deleted" /> 类型配合使用。
///     当数据被软删除时，系统将自动设置此属性的值。
/// </summary>
/// <param name="Value">数据的已删除时间，使用 <see cref="DateTimeOffset" /> 类型</param>
public record DeletedTime(DateTimeOffset Value);