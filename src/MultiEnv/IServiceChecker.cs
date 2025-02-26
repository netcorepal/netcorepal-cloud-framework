namespace NetCorePal.Extensions.MultiEnv;

public interface IServiceChecker
{
    ValueTask<bool> ServiceExist(string serviceName);


    string GetEnvServiceName(string originalServiceName, string env);
}