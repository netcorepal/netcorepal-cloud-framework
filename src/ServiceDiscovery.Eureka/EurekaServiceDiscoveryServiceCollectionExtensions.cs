using NetCorePal.ServiceDiscovery;
using NetCorePal.ServiceDiscovery.Eureka;
using Steeltoe.Discovery.Eureka;
using Steeltoe.Discovery.Eureka.Transport;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EurekaServiceDiscoveryServiceCollectionExtensions
    {
        public static IServiceCollection AddEurekaServiceDiscovery(this IServiceCollection services, Action<EurekaProviderOption> configAction)
        {
            
            services.Configure(configAction);

            var options = new EurekaProviderOption();
            configAction(options);
            services.Configure<EurekaClientOptions>(eco=> 
            {
                eco.ShouldOnDemandUpdateStatusChange = true;
                eco.ServiceUrl = options.ServerUrl;
                eco.ShouldRegisterWithEureka = options.RegisterService;
                eco.ShouldFilterOnlyUpInstances = options.OnlyUpInstances;
                eco.ShouldGZipContent = options.GZipContent;
                eco.EurekaServerConnectTimeoutSeconds = options.ConnectTimeoutSeconds;
                eco.ValidateCertificates = options.ValidateCertificates;
                eco.RegistryFetchIntervalSeconds = options.FetchIntervalSeconds;
                eco.ShouldGZipContent = true;
            });

            services.Configure<EurekaInstanceOptions>(eic => {
                eic.AppName = options.AppName;
            });

            var client = new HttpClient(new SocketsHttpHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip }); //fix issue: System.Text.Json.JsonException: '0x1F' is an invalid start of a value. Path: $ | LineNumber: 0 | BytePositionInLine: 0. see https://github.com/SteeltoeOSS/Steeltoe/issues/865 
            client.DefaultRequestHeaders.Add("accept", "application/json");
            services.AddSingleton<IEurekaHttpClient>(p => new EurekaHttpClient(p.GetService<IOptions<EurekaClientOptions>>()?.Value, client, p.GetService<ILoggerFactory>()));
            services.AddSingleton<EurekaDiscoveryClient>();
            services.AddSingleton<EurekaApplicationInfoManager>();
            services.AddSingleton<IServiceDiscoveryProvider, EurekaServiceDiscoveryProvider>();
            return services;
        }
    }


    class ms : HttpMessageHandler
    {




        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            return Task.FromResult(new HttpResponseMessage());
            
        }
    }
}
