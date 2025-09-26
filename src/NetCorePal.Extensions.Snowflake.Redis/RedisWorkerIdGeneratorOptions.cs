using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NetCorePal.Extensions.Snowflake.Redis
{
    public class RedisWorkerIdGeneratorOptions
    {
        public string AppName { get; set; } = "myAppName";
        public string RedisKeyPrefix { get; set; } = string.Empty;
        public int SessionTtlSeconds { get; set; } = 60;
        public int SessionRefreshIntervalSeconds { get; set; } = 15;
        
        public HealthStatus UnhealthyStatus { get; set; } = HealthStatus.Unhealthy;
    }
}