using System;

namespace NetCorePal.Extensions.Primitives
{
    /// <summary>
    /// 表示一个时钟，可以获取当前时间
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// 获取当前Utc时间,相当于DateTime.UtcNow
        /// </summary>
        DateTime UtcNow { get; }

        /// <summary>
        /// 获取当前机器时区的时间,相当于DateTime.Now
        /// </summary>
        DateTime Now { get; }
    }
}