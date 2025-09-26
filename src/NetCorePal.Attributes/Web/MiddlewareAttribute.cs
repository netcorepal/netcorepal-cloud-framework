using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Attributes.Web
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MiddlewareAttribute : Attribute
    {
        /// <summary>
        /// 映射地址
        /// </summary>
        public string MapPath { get; set; } = string.Empty;

        /// <summary>
        /// 中间间注册顺序
        /// </summary>
        public int Order { get; set; }
    }
}
