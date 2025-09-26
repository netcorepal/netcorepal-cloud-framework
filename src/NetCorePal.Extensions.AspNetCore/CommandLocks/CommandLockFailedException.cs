namespace NetCorePal.Extensions.AspNetCore.CommandLocks;

/// <inheritdoc />
#pragma warning disable S3925
public sealed class CommandLockFailedException : Exception
#pragma warning restore S3925
{
    public CommandLockFailedException(string message, string failedKey) : base(message)
    {
        FailedKey = failedKey;
    }

    public string FailedKey { get; private set; }
}