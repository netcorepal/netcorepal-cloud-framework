using NetCorePal.Extensions.Domain;

namespace NetCorePal.Web.Domain.DomainEvents;

public record OrderPaidDomainEvent(Order order) : IDomainEvent;