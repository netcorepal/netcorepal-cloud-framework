using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConditionalAttribute : Attribute
    {
        /// <summary>
        /// 实现了<see cref="IRegistrationCondition"/> 接口的类型
        /// </summary>
        Type? RegistrationConditionType { get; set; }
    }
}
