using DotNetCore.CAP;
using NetCorePal.Context.Diagnostics.CAP;

namespace NetCorePal.Extensions.DependencyInjection;

public static class CapBuilderExtensions
{
    public static CapBuilder AddEnvCapFilter(this CapBuilder builder)
    {
        builder.AddSubscribeFilter<CapContextSubscribeFilter>();
        return builder;
    }
}