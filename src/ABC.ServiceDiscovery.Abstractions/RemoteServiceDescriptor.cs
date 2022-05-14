namespace ABC.ServiceDiscovery.Abstractions
{
    /// <summary>
    /// 远程服务描述
    /// </summary>
    public class RemoteServiceDescriptor : IRemoteServiceDescriptor
    {

        public RemoteServiceDescriptor(string serviceName, string instanceId, string host, int port, bool isSecure, Uri uri, Dictionary<string, string> metadata)
        {
            this.ServiceName = serviceName;
            this.InstanceId = instanceId;
            this.Host = host;
            this.Port = port;
            this.Uri = uri;
            this.Metadata = metadata;
        }


        /// <summary>
        /// 服务名  
        /// </summary>
        public string ServiceName { get; private set; }

        public string InstanceId { get; private set; }

        public string Host { get; private set; }

        public int Port { get; private set; }

        public bool IsSecure { get; private set; }

        public Uri Uri { get; private set; }

        /// <summary>
        /// 元数据
        /// </summary>
        public Dictionary<string, string> Metadata { get; private set; }



    }
}
