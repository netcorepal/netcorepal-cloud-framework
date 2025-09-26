using DotNetCore.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.CAP;

public class EnvCapSubscribeAttribute(string name, bool isPartial = false) : CapSubscribeAttribute(name, isPartial)
{
    public static string ServiceEnv { get; set; } = string.Empty;

    public string EnvGroup
    {
        get => Group;
        set => Group = string.IsNullOrEmpty(ServiceEnv) ? value : $"{value}.{ServiceEnv}";
    }
}