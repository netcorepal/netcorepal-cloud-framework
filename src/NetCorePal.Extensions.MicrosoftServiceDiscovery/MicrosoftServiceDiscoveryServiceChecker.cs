using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using NetCorePal.Context;
using NetCorePal.Extensions.MultiEnv;

namespace NetCorePal.Extensions.MicrosoftServiceDiscovery;

internal class MicrosoftServiceDiscoveryServiceChecker : IServiceChecker
{
    private readonly ConcurrentDictionary<string, ServiceEndpointWatcher> _watchers = new();
    private readonly ServiceEndpointWatcherFactory _serviceEndpointWatcherFactory;
    private readonly MultiEnvMicrosoftServiceDiscoveryOptions _options;

    public MicrosoftServiceDiscoveryServiceChecker(IOptions<MultiEnvMicrosoftServiceDiscoveryOptions> options,
        ServiceEndpointWatcherFactory factory)
    {
        _options = options.Value;
        _serviceEndpointWatcherFactory = factory;
    }

    ServiceEndpointWatcher GetWatcher(string serviceName)
    {
        return _watchers.GetOrAdd(serviceName, serviceName =>
        {
            var watcher = _serviceEndpointWatcherFactory.CreateWatcher(serviceName);
            watcher.Start();
            return watcher;
        });
    }

    public async ValueTask<bool> ServiceExist(string serviceName)
    {
        var watcher = GetWatcher(serviceName);
        var endpoints = await watcher.GetEndpointsAsync();
        return endpoints.Endpoints.Count > 0;
    }

    public string GetEnvServiceName(string originalServiceName, string env)
    {
        var host = new Uri(originalServiceName).Host;
        var envService = originalServiceName.Replace(host,
            _options.EnvServiceNameFunc(host, env));
        return envService;
    }
}