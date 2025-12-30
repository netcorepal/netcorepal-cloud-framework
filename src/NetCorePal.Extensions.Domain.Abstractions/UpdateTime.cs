namespace NetCorePal.Extensions.Domain;

/// <summary>
/// 表示数据的最新更新时间，如果实体发生变化，DbContext 会自动更新这个字段
/// </summary>
/// <param name="Value">更新时间的值</param>
public record UpdateTime(DateTimeOffset Value)
{
    // Comparison operators between UpdateTime and UpdateTime
    public static bool operator <=(UpdateTime left, UpdateTime right) => left.Value <= right.Value;
    public static bool operator >=(UpdateTime left, UpdateTime right) => left.Value >= right.Value;
    public static bool operator <(UpdateTime left, UpdateTime right) => left.Value < right.Value;
    public static bool operator >(UpdateTime left, UpdateTime right) => left.Value > right.Value;
}