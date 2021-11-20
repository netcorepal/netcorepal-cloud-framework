namespace ABC.ServiceDiscovery.Abstractions
{
    /// <summary>
    /// 远程服务描述
    /// </summary>
    public class RemoteServiceDescriptor
    {
        /// <summary>
        /// 服务名  
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// 服务地址
        /// </summary>
        public string Address { get; set; } = null!;

        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; } = null!;

        /// <summary>
        /// 元数据
        /// </summary>
        public Dictionary<string, string> Metadatas { get; set; } = null!;
    }
}
