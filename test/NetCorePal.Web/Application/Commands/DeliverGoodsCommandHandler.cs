using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Web.Application.Commands
{
    public class DeliverGoodsCommandHandler : ICommandHandler<DeliverGoodsCommand, DeliverRecordId>
    {
        readonly IDeliverRecordRepository _deliverRecordRepository;
        public DeliverGoodsCommandHandler(IDeliverRecordRepository deliverRecordRepository)
        {
            _deliverRecordRepository = deliverRecordRepository;
        }

        public Task<DeliverRecordId> Handle(DeliverGoodsCommand request, CancellationToken cancellationToken)
        {
            var record = new DeliverRecord(request.OrderId);
            _deliverRecordRepository.Add(record);
            return Task.FromResult(record.Id);
        }
    }
}
