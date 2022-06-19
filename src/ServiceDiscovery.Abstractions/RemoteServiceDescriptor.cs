namespace NetCorePal.ServiceDiscovery
{
    /// <summary>
    /// 远程服务描述
    /// </summary>
    public sealed record  class RemoteServiceDescriptor : IDestination
    {

        public RemoteServiceDescriptor(string serviceName, string instanceId, string address, IReadOnlyDictionary<string, string> metadata)
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
