using NetCorePal.Attributes.DependencyInjection;
using System;

namespace NetCorePal.Attributes.Background
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class JobAttribute : ComponentAttribute
    {
        /// <summary>
        ///  废弃后台任务
        /// </summary>
        public bool Obsolete { get; set; } = false;
    }
}
