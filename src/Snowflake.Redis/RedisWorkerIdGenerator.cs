using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace NetCorePal.Extensions.Snowflake.Redis
{
    public sealed class RedisWorkerIdGenerator : BackgroundService, IWorkIdGenerator, IHealthCheck
    {
        private readonly ILogger<RedisWorkerIdGenerator> _logger;

        private readonly RedisWorkerIdGeneratorOptions _options;


        public bool IsHealth { get; private set; } = false;


        //  work id
        private readonly long _workId;

        /// <summary>
        /// workid 识别名，由MachineName+GUID组成
        /// </summary>
        private readonly string _workerName;

        private readonly IConnectionMultiplexer _connectionMultiplexer;

        private readonly IDatabase _database;

        public RedisWorkerIdGenerator(ILogger<RedisWorkerIdGenerator> logger,
            IOptions<RedisWorkerIdGeneratorOptions> options,
            IConnectionMultiplexer connectionMultiplexer)
        {
            _workerName = $"{Environment.MachineName}-{Guid.NewGuid()}";
            _logger = logger;
            _options = options.Value;
            _connectionMultiplexer = connectionMultiplexer;
            _database = connectionMultiplexer.GetDatabase();
            _workId = GenWorkId().Result;
            _logger.LogInformation("workerid got, workerId: {workerId} , identity name: {workerName}",
                _workId, _workerName);
        }


        private async Task<long> GenWorkId()
        {
            for (int i = 0; i < 32; i++)
            {
                if (await TryLockWorkId(i))
                {
                    this.IsHealth = true;
                    return i;
                }
            }

#pragma warning disable S112
            throw new Exception("初始化workerId失败");
#pragma warning restore S112
        }


        private async Task<bool> TryLockWorkId(long workId)
        {
            try
            {
                string key = GetWorkerIdKey(workId);
                var success = await _database.StringSetAsync(key, _workerName,
                    TimeSpan.FromSeconds(_options.SessionTtlSeconds), When.NotExists);
                return success;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "尝试获取workerId时出错");
                this.IsHealth = false;
                throw;
            }
        }

        public long GetId() => _workId;

        public async Task ReleaseId()
        {
            var key = GetWorkerIdKey(_workId);
            await _database.KeyDeleteAsync(key);
        }

        public async Task Refresh(CancellationToken stoppingToken = default)
        {
            var key = GetWorkerIdKey(_workId);
            var value = await _database.StringGetAsync(key);

            if (_workerName == value)
            {
                await _database.StringSetAsync(key, _workerName, TimeSpan.FromSeconds(_options.SessionTtlSeconds));
                IsHealth = true;
                _logger.LogInformation("刷新workerIdkey：{key} 的过期时间成功，workerName: {workerName}", key, _workerName);
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                var success = await _database.StringSetAsync(key, _workerName,
                    TimeSpan.FromSeconds(_options.SessionTtlSeconds),
                    When.NotExists);
                if (success)
                {
                    _logger.LogInformation("刷新workerIdkey：{key} 的过期时间成功，workerName: {workerName}", key, _workerName);
                    IsHealth = true;
                }
                else
                {
                    _logger.LogWarning("刷新workerIdkey：{key} 已被wokername：{value} 抢占，本workername为：{workerName}",
                        key, value, _workerName);
                    IsHealth = false;
                    throw new WorkerIdConflictException($"抢占workerId:{_workId}失败");
                }
            }
            else
            {
                _logger.LogWarning("刷新workerIdkey：{key} 已被wokername：{value} 抢占，本workername为：{workerName}",
                    key, value, _workerName);
                IsHealth = false;
                throw new WorkerIdConflictException($"抢占workerId:{_workId}失败");
            }
        }

        public string GetWorkerIdKey()
        {
            ArgumentNullException.ThrowIfNull(_workId);
            return GetWorkerIdKey(_workId);
        }

        private string GetWorkerIdKey(long workId)
        {
            if (string.IsNullOrEmpty(_options.RedisKeyPrefix))
            {
                return $"workerid:{_options.AppName}:{workId}";
            }
            else
            {
                return $"{_options.RedisKeyPrefix}:workerid:{_options.AppName}:{workId}";
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

        #region HealthCheck

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (this.IsHealth)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            else
            {
                return Task.FromResult(new HealthCheckResult(_options.UnhealthyStatus,
                    $"workerId: {_workId} 对应的consul key锁定失败"));
            }
        }

        #endregion
    }
}