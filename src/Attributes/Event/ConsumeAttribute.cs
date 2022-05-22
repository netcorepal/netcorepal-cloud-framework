using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Attributes.Event
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ConsumeAttribute : Attribute
    {
        public ConsumeAttribute(Type eventType)
        {
            EventType = eventType;
        }

        /// <summary>
        /// 事件类型
        /// </summary>
        public Type EventType { get; set; }
    }
}
