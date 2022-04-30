using System;

namespace ABC.Extensions.Primitives
{
    /// <summary>
    /// 系统时间时钟
    /// </summary>
    public class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
