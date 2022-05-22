using System;

namespace NetCorePal.Extensions.Primitives
{
    /// <summary>
    /// 表示一个时钟，可以获取当前时间
    /// </summary>
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
