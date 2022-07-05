using System;
namespace NetCorePal.ServiceDiscovery.Eureka
{
    public class EurekaProviderOption
    {
        public string AppName { get; set; } = default!;

        public string ServerUrl { get; set; } = default!;

        public bool RegisterService { get; set; } = true;

        public bool OnlyUpInstances { get; set; } = false;

        public bool GZipContent { get; set; } = true;

        public int ConnectTimeoutSeconds { get; set; } = 30;

        public bool ValidateCertificates { get; set; }

        public int FetchIntervalSeconds { get; set; } = 10;
    }
}

