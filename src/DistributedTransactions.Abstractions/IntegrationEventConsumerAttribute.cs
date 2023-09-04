using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.DistributedTransactions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class IntegrationEventConsumerAttribute : Attribute
    {
        public IntegrationEventConsumerAttribute(string eventName, string groupName = "")
        {
            EventName = eventName;
            GroupName = groupName;
        }
        /// <summary>
        /// 事件类型
        /// </summary>
        public string EventName { get; private set; }
        public string GroupName { get; private set; }
    }
}
