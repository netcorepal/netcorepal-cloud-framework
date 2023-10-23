namespace NetCorePal.Extensions.Snowflake.Consul
{
    public class ConsulWorkerIdGeneratorOptions
    {
        public string AppName { get; set; } = "myAppName";
        public string ConsulKeyPrefix { get; set; } = string.Empty;
        public int SessionTtlSeconds { get; set; } = 60;
        public int SessionRefreshIntervalSeconds { get; set; } = 15;
    }
}