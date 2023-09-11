using Microsoft.Extensions.Primitives;

namespace NetCorePal.ServiceDiscovery
{
    /// <summary>
    /// 服务提供者接口
    /// </summary>
    public interface IServiceDiscoveryProvider
    {
        /// <summary>
        /// 获取服务
        /// </summary>
        /// <returns>服务信息</returns>
        IEnumerable<IServiceCluster> Clusters { get; }

        /// <summary>
        /// 更新token
        /// </summary>
        /// <returns></returns>
        IChangeToken GetReloadToken();

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RegisterAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 注销服务
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeregisterAsync(CancellationToken cancellationToken = default(CancellationToken));

    }
}
