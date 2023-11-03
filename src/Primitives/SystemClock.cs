using System;

namespace NetCorePal.Extensions.Primitives
{
    /// <summary>
    /// 系统时间时钟
    /// </summary>
    public sealed class SystemClock : IClock
    {
        /// <summary>
        /// 获取当前Utc时间
        /// </summary>
        public DateTime UtcNow => DateTime.UtcNow;

        /// <summary>
        /// 获取当前机器时区的时间
        /// </summary>
        public DateTime Now => DateTime.Now;

        /// <summary>
        /// 获取或设置默认时钟实例
        /// </summary>
        public static IClock DefaultClock { get; set; } = new SystemClock();

        /// <summary>
        /// 通过默认时钟获取当前Utc时间
        /// </summary>
        public static DateTime DefaultUtcNow => DefaultClock.UtcNow;

        /// <summary>
        /// 通过默认时钟获取当前机器时区的时间
        /// </summary>
        public static DateTime DefaultNow => DefaultClock.Now;
    }
}