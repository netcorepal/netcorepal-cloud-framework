

using NetCorePal.Web.Domain;

namespace NetCorePal.Web.Application.IntegrationEventHandlers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="OrderId"></param>
    public record OrderPaidIntegrationEvent(OrderId OrderId);
}
