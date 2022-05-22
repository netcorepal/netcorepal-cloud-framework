using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Attributes.Event
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
    public class PublishAttribute : Attribute
    {
        public PublishAttribute(Type eventType)
        {
            EventType = eventType;
        }

        /// <summary>
        /// 事件类型
        /// </summary>
        public Type EventType { get; set; }
    }
}
