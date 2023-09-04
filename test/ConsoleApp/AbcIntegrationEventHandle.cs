using DotNetCore.CAP.Messages;
using DotNetCore.CAP;
using NetCorePal.Extensions.DistributedTransactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.ConsoleApp
{

    public record AbcEventData(long Id);


    [IntegrationEventConsumer("Abcssss", groupName: "")]
    public class AbcIntegrationEventHandle : IIntegrationEventHandle<AbcEventData>
    {
        public Task HandleAsync(AbcEventData eventData, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException(); 
        }
    }

    public class AsyncSubscriber : ICapSubscribe
    {
        readonly AbcIntegrationEventHandle _handler;

        public AsyncSubscriber(AbcIntegrationEventHandle handler)
        {
            _handler = handler;
        }


        [CapSubscribe("name", Group = "")]
        public Task ProcessAsync(AbcEventData message, CancellationToken cancellationToken)
        {
            return _handler.HandleAsync(message, cancellationToken);
        }
    }

}
