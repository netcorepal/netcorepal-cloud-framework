namespace ABC.ServiceDiscovery.K8S
{
    public class K8SProviderOption
    {
        /// <summary>
        /// k8s 服务地址
        /// </summary>
        public string ServerAddress { get; set; } = null!;

        /// <summary>
        /// 
        /// </summary>
        public string CertificateAuthorityData { get; set; } = null!;

        /// <summary>
        /// 
        /// </summary>
        public string ClientCertificateData { get; set; } = null!;

        /// <summary>
        /// 
        /// </summary>
        public string ClientKeyData { get; set; } = null!;

        /// <summary>
        /// 搜索标签
        /// 如：servicediscovery=true
        /// </summary>
        public string LabelOfSearch { get; set; } = null!;

        /// <summary>
        /// 搜索标签
        /// 如：servicediscovery.app
        /// </summary>
        public string LabelKeyOfServiceName { get; set; } = null!;

        /// <summary>
        /// 版本key
        /// 如：servicediscovery.version
        /// </summary>
        public string LabelKeyOfVersion { get; set; } = null!;
    }
}
