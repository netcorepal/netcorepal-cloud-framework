using NetCorePal.Extensions.Domain;

namespace NetCorePal.Web.Domain.DomainEvents
{
    public class OrderCreatedDomainEvent : IDomainEvent
    {
        public OrderCreatedDomainEvent(Order order)
        {
            Order = order;
        }

        public Order Order { get; }

    }
}
