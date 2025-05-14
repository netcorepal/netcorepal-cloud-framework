namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.Tenant;

/// <summary>
/// 租户数据源提供程序，单例模式
/// </summary>
public interface ITenantDataSourceProvider
{
    
    /// <summary>
    /// 根据租户Id 获取对应的数据源名称
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    string GetDataSourceName(string tenantId);
}