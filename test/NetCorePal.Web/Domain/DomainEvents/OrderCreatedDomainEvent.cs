using NetCorePal.Extensions.Domain;

namespace NetCorePal.Web.Domain.DomainEvents
{
    public class OrderCreatedDomainEvent(Order order) : IDomainEvent
    {
        public Order Order { get; } = order;

    }
}
