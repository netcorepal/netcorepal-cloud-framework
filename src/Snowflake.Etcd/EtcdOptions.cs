namespace NetCorePal.Extensions.Snowflake.Etcd
{
    public class EtcdOptions
    {
        public string Host { get; set; } = null!;

        public int Port { get; set; } = 2379;

        public string CACert { get; set; } = string.Empty;

        public string ClientCert { get; set; } = string.Empty;

        public string ClientKey { get; set; } = string.Empty;

        public bool PublicRootCA { get; set; }
    }
}