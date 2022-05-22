using Microsoft.Extensions.DependencyInjection;
using System;

namespace NetCorePal.Attributes
{
    /// <summary>
    /// 组件特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ComponentAttribute : Attribute
    {
        public ComponentAttribute()
        {

        }

        public ComponentAttribute(Type @as)
        {
            As = @as;
        }

        /// <summary>
        /// 注册服务生命周期
        /// </summary>
        public virtual ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

        /// <summary>
        /// 接口类型
        /// </summary>
        public virtual Type? As { get; set; }
    }
}
