using Medallion.Threading;
using NetCorePal.Extensions.DistributedLocks;
using NetCorePal.Extensions.DistributedLocks.Redis.Diagnostics;

namespace NetCorePal.Extensions.DistributedLocks.Redis;

internal sealed class RedisLockSynchronizationHandler : ILockSynchronizationHandler
{
    private readonly IDistributedSynchronizationHandle _handle;

    public RedisLockSynchronizationHandler(IDistributedSynchronizationHandle handle)
    {
        _handle = handle;
    }

    public CancellationToken HandleLostToken => _handle.HandleLostToken;

    public void Dispose()
    {
        Guid id = Guid.NewGuid();
        try
        {
            WriteReleaseBeginData(new ReleaseBeginData(id, "Dispose"));
            _handle.Dispose();
            WriteReleaseEndData(new ReleaseEndData(id, $"Dispose"));
        }
        catch (Exception ex)
        {
            WriteReleaseErrorData(new ReleaseErrorData(id, "Dispose", ex));
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        Guid id = Guid.NewGuid();
        try
        {
            WriteReleaseBeginData(new ReleaseBeginData(id, "DisposeAsync"));
            await _handle.DisposeAsync();
            WriteReleaseEndData(new ReleaseEndData(id, $"Dispose"));
        }
        catch (Exception ex)
        {
            WriteReleaseErrorData(new ReleaseErrorData(id, "DisposeAsync", ex));
            throw;
        }
    }


    void WriteReleaseBeginData(ReleaseBeginData data)
    {
        if (NetCorePalRedisDiagnosticListenerNames.DiagnosticListener.IsEnabled(NetCorePalRedisDiagnosticListenerNames
                .ReleaseBegin))
        {
            NetCorePalRedisDiagnosticListenerNames.DiagnosticListener.Write(
                NetCorePalRedisDiagnosticListenerNames.ReleaseBegin, data);
        }
    }

    void WriteReleaseEndData(ReleaseEndData data)
    {
        if (NetCorePalRedisDiagnosticListenerNames.DiagnosticListener.IsEnabled(NetCorePalRedisDiagnosticListenerNames
                .ReleaseEnd))
        {
            NetCorePalRedisDiagnosticListenerNames.DiagnosticListener.Write(
                NetCorePalRedisDiagnosticListenerNames.ReleaseEnd, data);
        }
    }

    void WriteReleaseErrorData(ReleaseErrorData data)
    {
        if (NetCorePalRedisDiagnosticListenerNames.DiagnosticListener.IsEnabled(NetCorePalRedisDiagnosticListenerNames
                .ReleaseError))
        {
            NetCorePalRedisDiagnosticListenerNames.DiagnosticListener.Write(
                NetCorePalRedisDiagnosticListenerNames.ReleaseError, data);
        }
    }
}