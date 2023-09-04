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


    [IntegrationEventConsumer("Abcssss", "g2")]
    public class AbcIntegrationEventHandle : IIntegrationEventHandler<AbcEventData>
    {
        public Task HandleAsync(AbcEventData eventData, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException(); 
        }
    }



    public class EfgIntegrationEventHandle : IIntegrationEventHandler<AbcEventData>
    {
        public Task HandleAsync(AbcEventData eventData, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
