using System.Text.Json;
using k8s;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DependencyInjection;
using NetCorePal.Extensions.ServiceDiscovery.K8s;
using Testcontainers.K3s;

namespace NetCorePal.Extensions.ServiceDiscovery.K8s.UnitTests;

public class K8SServiceDiscoveryProviderTests : IAsyncLifetime
{
    private readonly K3sContainer _k3sContainer =
        new K3sBuilder().Build();

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    private Kubernetes _k8SClient;
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    private const string kubeconfigPath = "kubecfg.cfg";

    public async Task InitializeAsync()
    {
        await _k3sContainer.StartAsync();
        await File.WriteAllTextAsync(kubeconfigPath, await _k3sContainer.GetKubeconfigAsync());

        _k8SClient = new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigFile(kubeconfigPath));
        var json = await File.ReadAllTextAsync("Services.json");
        var services = JsonSerializer.Deserialize<List<V1Service>>(json);

        if (services != null)
        {
            var namespaceList = services.GroupBy(p => p.Namespace(), p => p)
                .Where(p => !string.IsNullOrEmpty(p.Key))
                .Select(p => p.Key).ToList();
            foreach (var namespaceName in namespaceList)
            {
                var ns = new V1Namespace
                {
                    Metadata = new V1ObjectMeta
                    {
                        Name = namespaceName
                    }
                };
                await _k8SClient.CreateNamespaceAsync(ns);
            }

            foreach (V1Service service in services)
            {
                await _k8SClient.CreateNamespacedServiceAsync(service, service.Namespace());
            }
        }
    }

    public Task DisposeAsync()
    {
        return _k3sContainer.StopAsync();
    }

    private static ServiceProvider GetServiceProvider()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddOptions();
        services.AddLogging();
        services.AddK8SServiceDiscovery(options =>
        {
            options.KubeconfigPath = kubeconfigPath;
            options.ServiceNamespace = "test";
            options.LabelKeyOfServiceName = "app";
        });
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task GetK8SServiceDiscoveryProviderTests()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddOptions();
        services.AddLogging();
        services.AddK8SServiceDiscovery(options =>
        {
            options.KubeconfigPath = kubeconfigPath;
            options.ServiceNamespace = "test";
            options.LabelKeyOfServiceName = "app";
        });
        var serviceProvider = services.BuildServiceProvider();
        var provider = (K8SServiceDiscoveryProvider)serviceProvider.GetRequiredService<IServiceDiscoveryProvider>();
        Assert.NotNull(provider);
        await provider.LoadAsync();
    }

    [Fact]
    public async Task GetReloadTokenTest()
    {
        var serviceProvider = GetServiceProvider();
        var provider = (K8SServiceDiscoveryProvider)serviceProvider.GetRequiredService<IServiceDiscoveryProvider>();
        CancellationToken token = new();
        await provider.LoadAsync(token);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        provider.StartAsync(token); //开始监控
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

        Assert.NotEmpty(provider.Clusters);
        Assert.Equal(2, provider.Clusters.Count());

        await Task.Delay(1000);
        var reloadToken = provider.GetReloadToken();
        Assert.False(reloadToken.HasChanged);
        await _k8SClient.CoreV1.DeleteNamespacedServiceAsync("service2-main", "test");
        await Task.Delay(1000);
        Assert.True(reloadToken.HasChanged);
        Assert.Single(provider.Clusters);
    }
}