namespace NetCorePal.Extensions.AspNetCore.CommandLocks;

public class CommandLockFailedException(string message) : Exception(message: message)
{
    
}