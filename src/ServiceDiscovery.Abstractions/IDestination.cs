using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.ServiceDiscovery
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


    /// <summary>
    /// 远程服务描述
    /// </summary>
    public sealed record class Destination : IDestination
    {

        public Destination(string serviceName, string instanceId, string address, IReadOnlyDictionary<string, string> metadata)
        {
            ServiceName = serviceName;
            InstanceId = instanceId;
            Address = address;
            Metadata = metadata;
        }

        /// <summary>
        /// 服务名  
        /// </summary>
        public string ServiceName { get; private set; }

        public string InstanceId { get; private set; }


        /// <summary>
        /// 元数据
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; private set; }

        public string Address { get; private set; }

    }
}
