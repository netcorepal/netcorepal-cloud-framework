using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Web.Application.Commands
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="deliverRecordRepository"></param>
    public class DeliverGoodsCommandHandler(DeliverRecordRepository deliverRecordRepository) : ICommandHandler<DeliverGoodsCommand, DeliverRecordId>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<DeliverRecordId> Handle(DeliverGoodsCommand request, CancellationToken cancellationToken)
        {
            var record = new DeliverRecord(request.OrderId);
            deliverRecordRepository.Add(record);
            return Task.FromResult(record.Id);
        }
    }
}
