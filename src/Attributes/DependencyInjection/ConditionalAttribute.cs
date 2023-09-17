using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Attributes.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConditionalAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public Type? RegistrationConditionType { get; set; }
    }
}
