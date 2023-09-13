namespace NetCorePal.Extensions.ServiceDiscovery.K8s
{
    public class K8SProviderOption
    {
        public string ServiceNamespace { get; set; } = null!;

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

        public string EnvLabelKey { get; set; } = "env";

        public int? ServicePort { get; set; }

        public string ServicePortName { get; set; } = "http";

        public SchemeType ServiceScheme { get; set; } = SchemeType.Http;

        public string? KubeconfigPath { get; set; }

        public bool UseRelativePaths { get; set; } = true;

        /// <summary>
        /// 单位秒
        /// </summary>
        public int ReloadInterval { get; set; } = 60;
    }

    public enum SchemeType
    {
        Https,
        Http
    }
}