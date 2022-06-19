using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.ServiceDiscovery
{
    public interface IDestination
    {
        /// <summary>
        /// 服务标识
        /// </summary>
        string ServiceName { get; }
        /// <summary>
        /// 服务实例标识
        /// </summary>
        string InstanceId { get; }
        /// <summary>
        /// 服务地址
        /// </summary>
        string Address { get; }

        IReadOnlyDictionary<string, string> Metadata { get; }
    }
}
