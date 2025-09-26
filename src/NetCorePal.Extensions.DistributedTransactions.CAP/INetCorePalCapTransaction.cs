using DotNetCore.CAP;

namespace NetCorePal.Extensions.DistributedTransactions.CAP;

public interface INetCorePalCapTransaction : ICapTransaction, IAsyncDisposable
{
}