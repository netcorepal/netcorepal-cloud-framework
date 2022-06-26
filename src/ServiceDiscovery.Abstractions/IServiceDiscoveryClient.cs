using System;
using Microsoft.Extensions.Primitives;

namespace NetCorePal.ServiceDiscovery
{
    public interface IServiceDiscoveryClient
    {
        IReadOnlyDictionary<string, IServiceCluster> GetServiceClusters();

        /// <summary>
        /// 更新token
        /// </summary>
        /// <returns></returns>
        IChangeToken GetReloadToken();
    }
}

