using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore.Storage;

namespace NetCorePal.Extensions.DistributedTransactions.CAP;

public interface ICapTransactionFactory
{
    INetCorePalCapTransaction CreateCapTransaction();
}