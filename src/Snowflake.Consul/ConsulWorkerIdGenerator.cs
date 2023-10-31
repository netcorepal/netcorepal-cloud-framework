using System.Globalization;
using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetCorePal.Extensions.Snowflake.Consul
{
    public sealed class ConsulWorkerIdGenerator : BackgroundService, IWorkIdGenerator
    {
        private const string SessionName = "snowflake-workerid-session";
        private readonly TimeSpan _sessionTtl;
        private readonly ILogger<ConsulWorkerIdGenerator> _logger;

        private readonly ConsulWorkerIdGeneratorOptions _options;

        private string _sessionId = string.Empty;

        //  work id
        private readonly long? _workId;

        /// <summary>
        /// workid 识别名，由进程ID+MachineName组成
        /// </summary>
        private readonly string WorkId_Identity_Name;

        private readonly IConsulClient _consulClient;

        public ConsulWorkerIdGenerator(ILogger<ConsulWorkerIdGenerator> logger,
            IOptions<ConsulWorkerIdGeneratorOptions> options,
            IConsulClient consulClient)
        {
            WorkId_Identity_Name = $"{Environment.ProcessId},{Environment.MachineName}";
            _logger = logger;
            _options = options.Value;
            _sessionTtl = TimeSpan.FromSeconds(_options.SessionTtlSeconds);
            _consulClient = consulClient;
            _workId = GenWorkId().Result;
            _logger.LogInformation("workid got, identity name: {WorkId_Identity_Name}", WorkId_Identity_Name);
        }

        private async Task<string> CreateSession()
        {
            SessionEntry entry = new SessionEntry()
            {
                TTL = _sessionTtl,
                Behavior = SessionBehavior.Delete,
                Name = SessionName
            };
            var writeResult = await _consulClient.Session.Create(entry);
            return writeResult.Response;
        }


        private async Task<long> GenWorkId()
        {
            _sessionId = await CreateSession();
            for (int i = 0; i < 32; i++)
            {
                KVPair kvp = new(GetWorkerIdKey(i))
                {
                    Session = _sessionId,
                    Value = System.Text.Encoding.UTF8.GetBytes(
                        $"{WorkId_Identity_Name},{DateTime.Now.ToString(CultureInfo.InvariantCulture)}")
                };
                _logger.LogInformation("尝试使用key: {Key} 获取锁", kvp.Key);
                var result = await _consulClient.KV.Acquire(kvp);
                if (result.Response)
                {
                    _logger.LogInformation("获取到workerId：{workerId}", i);
                    return i;
                }
            }

            throw new Exception("初始化workerId失败");
        }

        public long GetId() => _workId ?? throw new ArgumentException("work id is missing");

        public async Task ReleaseId()
        {
            await _consulClient.Session.Destroy(_sessionId);
        }

        public async Task Refresh(CancellationToken stoppingToken = default)
        {
            var renewResult = await _consulClient.Session.Renew(_sessionId, stoppingToken);
            var result = await _consulClient.KV.Get(GetWorkerIdKey(), stoppingToken);
            _logger.LogInformation("成功刷新会话，sessionId：{sessionId}，值为 {value}, 当前workerid：{workId}.",
                _sessionId, result.Response, _workId);
        }

        private string GetWorkerIdKey()
        {
            ArgumentNullException.ThrowIfNull(_workId);
            return GetWorkerIdKey(_workId.Value);
        }

        private string GetWorkerIdKey(long workId)
        {
            if (string.IsNullOrEmpty(_options.ConsulKeyPrefix))
            {
                return $"snowflake/{_options.AppName}/workerId/{workId}";
            }
            else
            {
                return $"{_options.ConsulKeyPrefix}/snowflake/{_options.AppName}/workerId/{workId}";
            }
        }

        #region BackgroundService

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Refresh(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "刷新workerId时出错");
                }

                await Task.Delay(_options.SessionRefreshIntervalSeconds * 1000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await ReleaseId();
            await base.StopAsync(cancellationToken);
        }

        #endregion
    }
}