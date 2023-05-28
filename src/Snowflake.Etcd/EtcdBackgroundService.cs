using Microsoft.Extensions.Hosting;

namespace NetCorePal.Extensions.Snowflake.Etcd
{
    public class EtcdBackgroundService : BackgroundService
    {
        private readonly IWorkIdGenerator _workIdGenerator;

        public EtcdBackgroundService(IWorkIdGenerator workIdGenerator)
        {
            _workIdGenerator = workIdGenerator ?? throw new ArgumentNullException(nameof(workIdGenerator));

            if (_workIdGenerator is not EtcdWorkIdGenerator)
            {
                throw new ArgumentException("EtcdWorkIdGenerator implement support only", nameof(workIdGenerator));
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return ((EtcdWorkIdGenerator)_workIdGenerator).Refresh(stoppingToken);
        }
    }
}