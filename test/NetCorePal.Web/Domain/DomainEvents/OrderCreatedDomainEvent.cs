using NetCorePal.Extensions.Domain;

namespace NetCorePal.Web.Domain.DomainEvents
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="order"></param>
    public class OrderCreatedDomainEvent(Order order) : IDomainEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public Order Order { get; } = order;

    }
}
