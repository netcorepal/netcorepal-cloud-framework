namespace NetCorePal.Context;

/// <summary>
/// 租户Id
/// </summary>
/// <param name="Id"></param>
public record TenantId(long Id)
{
    public static implicit operator long(TenantId id) => id.Id;
    public static implicit operator TenantId(long id) => new(id);

    public static implicit operator string(TenantId id) => id.Id.ToString();
    public static implicit operator TenantId(string id) => new(long.Parse(id));

    public override string ToString()
    {
        return Id.ToString();
    }
}