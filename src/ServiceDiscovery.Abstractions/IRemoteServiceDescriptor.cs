using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.ServiceDiscovery
{
    public interface IRemoteServiceDescriptor
    {
        /// <summary>
        /// 服务标识
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// 服务实例标识
        /// </summary>
        string InstanceId { get; }

        string Host { get; }


        int Port { get; }


        bool IsSecure { get; }

        /// <summary>
        /// 服务地址
        /// </summary>
        Uri Uri { get; }

        Dictionary<string, string> Metadata { get; }
    }
}
