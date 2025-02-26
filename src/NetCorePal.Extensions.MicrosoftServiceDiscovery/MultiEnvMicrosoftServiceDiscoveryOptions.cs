namespace NetCorePal.Extensions.MicrosoftServiceDiscovery;

public class MultiEnvMicrosoftServiceDiscoveryOptions
{
    /// <summary>
    /// 默认的灰度环境服务名计算方法，输入服务名和环境名，输出灰度环境服务名，默认为 $"{hostName}-{env}"，例如：https://catalog:8080、https://catalog-v2:8080
    /// </summary>
    public Func<string, string, string> EnvServiceNameFunc { get; set; } =
        (hostName, env) => $"{hostName}-{env}";
}