using System;

namespace NetCorePal.Attributes
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
