

using NetCorePal.Web.Domain;

namespace NetCorePal.Web.Application.IntegrationEventHandlers
{
    public record OrderPaidIntegrationEvent(OrderId OrderId);
}
