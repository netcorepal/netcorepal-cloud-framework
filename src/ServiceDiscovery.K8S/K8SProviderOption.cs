namespace NetCorePal.ServiceDiscovery.K8S
{
    public class K8SProviderOption
    {
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
        /// 单位秒
        /// </summary>
        public int ReloadInterval { get; set; } = 60;

    }
}
