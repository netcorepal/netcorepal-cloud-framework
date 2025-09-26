using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Attributes.Modular
{
    /// <summary>
    /// 模块加载
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ModuleAttribute : Attribute
    {
        public ModuleAttribute(Type lifecycleType, int order = 0)
        {
            LifecycleType = lifecycleType;
            Order = order;
        }
        /// <summary>
        /// 模块生命周期控制
        /// </summary>
        public Type LifecycleType { get; set; }
        /// <summary>
        /// 模块加载顺序
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// 禁用模块加载
        /// </summary>
        public bool Disabled { get; set; } = false;
    }
}
