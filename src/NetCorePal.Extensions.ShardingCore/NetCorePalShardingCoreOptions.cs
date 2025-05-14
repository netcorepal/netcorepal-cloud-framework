namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

public class NetCorePalShardingCoreOptions
{
    public string DefaultDataSourceName { get; set; } = string.Empty;
    public List<string> AllDataSourceNames { get; set; } = [];
}