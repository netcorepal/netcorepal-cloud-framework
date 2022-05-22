using System;

namespace NetCorePal.Attributes.DependencyInjection
{
    /// <summary>
    /// 服务特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ServiceAttribute : ComponentAttribute
    {
        public ServiceAttribute()
        {
        }

        public ServiceAttribute(Type @as)
        {
            As = @as;
        }
    }
}
