using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd;
using Etcdserverpb;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetCorePal.Extensions.Snowflake.Etcd
{
    internal class EtcdWorkerIdGenerator : IWorkIdGenerator
    {
        private const int BIT = 12;
        private const string PREFIX = "/setnx/workId";
        private readonly EtcdOptions _options;
        private readonly ILogger<EtcdWorkerIdGenerator> _logger;

        //  work id
        private long? _workId;

        private long _leaseId;

        /// <summary>
        /// workid 识别名，由进程ID+MachineName组成
        /// </summary>
        private readonly string WorkId_Identity_Name;

        public EtcdWorkerIdGenerator(ILogger<EtcdWorkerIdGenerator> logger, IOptions<EtcdOptions> options)
        {
            WorkId_Identity_Name = $"{System.Diagnostics.Process.GetCurrentProcess().Id},{Environment.MachineName}";

            _logger = logger;

            _options = options.Value;

            _workId = GenWorkId();

            _logger.LogInformation($"workid got, identity name: {WorkId_Identity_Name}");
        }

        /// <summary>
        /// 尝试一次申请10个机器ID
        /// </summary>
        private const int STEP = 10;

        /// <summary>
        /// ttl 24 hours
        /// </summary>
        private const int TTL = 24 * 60 * 60;

        private const string WORK_ID_OCCUPIED_EXCEPTION_MESSAGE = "workid occupied";

        /// <summary>
        /// 续期频率
        /// </summary>
        /// <remarks>10分钟</remarks>
        private const int RefreshPeriodInSeconds = 600;

        readonly Func<EtcdOptions, EtcdClient> _etcdClientCreator = opt => new EtcdClient(opt.Host, opt.Port, opt.CACert, opt.ClientCert, opt.ClientKey, opt.PublicRootCA);

        private long GenWorkId()
        {
            using (var client = _etcdClientCreator(_options))
            {
                var bag = new ConcurrentBag<long>();     //  valid work id collection
                var seeds = new List<long>();            //  try to preemptive work id

                var max = Math.Pow(2, BIT);
                List<long> leaseIds = new();
                for (var q = 0; q < max;)
                {
                    for (var i = 0; i < STEP && q < max; i++)
                    {
                        seeds.Add(++q);
                    }

                    var stop = false;

                    var lease = client.LeaseGrant(new LeaseGrantRequest { ID = 0, TTL = TTL });
                    leaseIds.Add(lease.ID);
                    var loop = Parallel.ForEach(seeds, i =>
                    {
                        var req = new TxnRequest();

                        #region TXN with cas
                        req.Compare.Add(new Compare
                        {
                            Key = ByteString.CopyFromUtf8($"{PREFIX}{i}"),
                            Target = Compare.Types.CompareTarget.Create,
                            Result = Compare.Types.CompareResult.Equal,
                            Value = ByteString.CopyFromUtf8(WorkId_Identity_Name),
                        });

                        req.Success.Add(new RequestOp
                        {
                            RequestPut = new PutRequest
                            {
                                Key = ByteString.CopyFromUtf8($"{PREFIX}{i}"),
                                Value = ByteString.CopyFromUtf8(WorkId_Identity_Name),
                                Lease = lease.ID
                            }
                        });
                        #endregion

                        var rep = client.Transaction(req);  //  there is not enough time to about a task with cancellation token

                        //  work id (i) is valid
                        if (rep.Succeeded)
                        {
                            _leaseId = lease.ID;
                            stop = true;
                            bag.Add(i);
                        }
                    });

                    const int max_wait = 30000;
                    var loopTimes = 0;
                    while (!loop.IsCompleted)
                    {
                        if (++loopTimes >= max_wait)
                        {
                            break;
                        }
                        Thread.Sleep(1);
                    }

                    seeds.Clear();

                    if (stop)
                    {
                        break;
                    }
                }

                //  minimum valid work ids
                var workId = bag.Min();

                //  release unused work ids
                foreach (var r in bag)
                {
                    if (r != workId)
                    {
                        _ = client.Delete($"{PREFIX}{r}");
                    }
                }
                //remove lease
                foreach (var leaseId in leaseIds)
                {
                    if (leaseId != _leaseId)
                    {
                        _ = client.LeaseRevoke(new LeaseRevokeRequest() { ID = leaseId });
                    }
                }
                return workId;
            }
        }

        public long GetId() => _workId ?? throw new ArgumentException("work id is missing");

        private async Task ReleaseId()
        {
            using (var client = _etcdClientCreator(_options))
            {
                _ = await client.LeaseRevokeAsync(new LeaseRevokeRequest() { ID = _leaseId });
            }
        }

        public async Task Refresh(CancellationToken stoppingToken)
        {
            //默认ttl是24h，重试续约为8h，grpc异常重试周期为10m，重试次数(24*60-8*60)/10 = 96
            int count = 96;
            try
            {
                var client = _etcdClientCreator(_options);
                //自动续约
                await TryLeaseKeepAlive(client, count, stoppingToken);
            }
            catch (OperationCanceledException e)
            {
                //优雅退出，主动释放key
                _logger.Log(LogLevel.Information, e.Message);
                await ReleaseId();
            }
            catch (Exception e)
            {
                //其他异常
                _logger.Log(LogLevel.Error, e.Message);
            }
        }

        /// <summary>
        /// 尝试续约
        /// </summary>
        /// <param name="etcdClient"></param>
        /// <param name="count"></param>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task TryLeaseKeepAlive(EtcdClient etcdClient, int count, CancellationToken stoppingToken)
        {
            try
            {
                //默认ttl的1/3的时间进行续期
                await etcdClient.LeaseKeepAlive(_leaseId, stoppingToken);
            }
            catch (Grpc.Core.RpcException e)
            {
                //远程连接失败，进行重试
                if (count > 0)
                {
                    //续约失败支持持久化一段时间
                    _logger.Log(LogLevel.Error, e.Message);
                    // 续期失败，默认10分钟重试一次
                    await Task.Delay(10 * 60 * 1000);
                    count--;
                    await TryLeaseKeepAlive(etcdClient, count, stoppingToken);
                }
            }
        }
    }
}
