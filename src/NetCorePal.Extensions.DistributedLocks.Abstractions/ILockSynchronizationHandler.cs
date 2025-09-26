namespace NetCorePal.Extensions.DistributedLocks;

public interface ILockSynchronizationHandler : IDisposable, IAsyncDisposable
{
    CancellationToken HandleLostToken { get; }
}