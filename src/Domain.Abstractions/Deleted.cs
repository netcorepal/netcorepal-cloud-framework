namespace NetCorePal.Extensions.Domain;

/// <summary>
///     用于标记对象是否已被删除。
/// </summary>
/// <param name="Value">一个布尔值，指示对象是否已被删除，默认值为 false。</param>
public record Deleted(bool Value = false)
{
    public static implicit operator Deleted(bool value)
    {
        return new Deleted(value);
    }

    public static implicit operator bool(Deleted deleted)
    {
        return deleted.Value;
    }
}