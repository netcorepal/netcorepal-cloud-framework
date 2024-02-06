using NetCorePal.Extensions.Domain;

namespace NetCorePal.Web.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public partial record DeliverRecordId : IInt64StronglyTypedId;

    /// <summary>
    /// 
    /// </summary>
    public class DeliverRecord : Entity<DeliverRecordId>, IAggregateRoot
    {
        /// <summary>
        /// 
        /// </summary>
        protected DeliverRecord() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        public DeliverRecord(OrderId orderId)
        {
            this.OrderId = orderId;
        }

        /// <summary>
        /// 
        /// </summary>
        public OrderId OrderId { get; private set; } = default!;
    }
}
