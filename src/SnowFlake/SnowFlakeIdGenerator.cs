namespace NetCorePal.Extensions.SnowFlake
{
    public class SnowFlakeIdGenerator
    {
        private const long TwEpoch = 1604394839825L;//2020-11-3 9:14:15 +00:00

        private const int WorkerIdBits = 12;
        private const int SequenceBits = 10;

        private const int WorkerIdShift = SequenceBits;
        private const int TimestampLeftShift = SequenceBits + WorkerIdBits;
        private const long MaxSequence = -1L ^ (-1L << SequenceBits);

        private long _sequence = 0L;
        private readonly int _clockBackwardsInMinutes;
        private long _lastTimestamp = -1L;
        private long _workerId;
        private readonly object __lock = new object();

        /// <summary>
        /// 基于Twitter的snowflake算法
        /// </summary>
        /// <param name="sequence">初始序列</param>
        /// <param name="clockBackwardsInMinutes">时钟回拨容忍上限</param>
        public SnowFlakeIdGenerator(long workerId, long sequence = 0L, int clockBackwardsInMinutes = 2)
        {
            _workerId = workerId;
            _sequence = sequence;
            _clockBackwardsInMinutes = clockBackwardsInMinutes;
        }

        /// <summary>
        /// 获取下一个Id，该方法线程安全
        /// </summary>
        /// <returns></returns>
        public long NextId()
        {
            lock (__lock)
            {
                var timestamp = TimeGen();

                //  时钟回拨检测：超过2分钟，则强制抛出异常
                if (TimeSpan.FromMilliseconds(_lastTimestamp - timestamp) >= TimeSpan.FromMinutes(_clockBackwardsInMinutes))
                {
                    throw new NotSupportedException($"时钟回拨超过容忍上限{_clockBackwardsInMinutes}分钟");
                }

                while (timestamp < _lastTimestamp)//解决时钟回拨
                {
                    Thread.Sleep(1);
                    timestamp = TimeGen();
                }

                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & MaxSequence;
                    if (_sequence == 0)
                    {
                        timestamp = TilNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }
                _lastTimestamp = timestamp;
                return ((timestamp - TwEpoch) << TimestampLeftShift) |
                                   (_workerId << WorkerIdShift) | _sequence;
            }
        }

        private long TilNextMillis(long lastTimestamp)
        {
            var timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
                Thread.Sleep(1);
            }
            return timestamp;
        }

        //获取Unix时间戳（毫秒）
        private long TimeGen()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}