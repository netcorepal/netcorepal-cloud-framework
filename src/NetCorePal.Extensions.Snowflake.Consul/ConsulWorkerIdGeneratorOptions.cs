using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NetCorePal.Extensions.Snowflake.Consul
{
    public class ConsulWorkerIdGeneratorOptions
    {
        public string AppName { get; set; } = "myAppName";
        public string ConsulKeyPrefix { get; set; } = string.Empty;
        public int SessionTtlSeconds { get; set; } = 60;
        public int SessionRefreshIntervalSeconds { get; set; } = 15;
        
        public HealthStatus UnhealthyStatus { get; set; } = HealthStatus.Unhealthy;
    }
}