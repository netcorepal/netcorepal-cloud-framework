using DotNetCore.CAP;
using DotNetCore.CAP.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.DistributedTransactions.CAP
{
    public class NetCorePalCapTransactionFactory(ICapPublisher capPublisher)
        : ICapTransactionFactory
    {
        public INetCorePalCapTransaction CreateCapTransaction()
        {
            return ActivatorUtilities.CreateInstance<NetCorePalCapTransaction>(capPublisher.ServiceProvider);
            
        }
    }
}