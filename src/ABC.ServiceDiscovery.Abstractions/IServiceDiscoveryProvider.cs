using Microsoft.Extensions.Primitives;

namespace ABC.ServiceDiscovery.Abstractions
{
    /// <summary>
    /// 服务提供者接口
    /// </summary>
    internal interface IServiceDiscoveryProvider
    {
        /// <summary>
        /// 获取服务
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <param name="version">版本</param>
        /// <returns>服务信息</returns>
        IEnumerable<RemoteServiceDescriptor> Get(string serviceName, string version);

        /// <summary>
        /// 设置服务
        /// </summary>
        /// <param name="remoteService"></param>
        void Set(RemoteServiceDescriptor remoteService);

        /// <summary>
        /// 更新token
        /// </summary>
        /// <returns></returns>
        IChangeToken GetReloadToken();

        /// <summary>
        /// 加载数据
        /// </summary>
        void Load();
    }
}
