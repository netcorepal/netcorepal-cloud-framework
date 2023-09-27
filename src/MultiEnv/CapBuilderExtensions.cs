using DotNetCore.CAP;
using NetCorePal.Extensions.MultiEnv;

namespace NetCorePal.Extensions.DependencyInjection;

public static class CapBuilderExtensions
{
    public static CapBuilder AddEnvCapFilter(this CapBuilder builder)
    {
        builder.AddSubscribeFilter<EnvCapSubscribeFilter>();
        return builder;
    }
}