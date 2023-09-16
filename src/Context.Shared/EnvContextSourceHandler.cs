namespace NetCorePal.Context;

public class EnvContextSourceHandler : IContextSourceHandler
{
    public Type ContextType => typeof(EnvContext);

    public object? Extract(IContextSource source)
    {
        var env = source.Get(EnvContext.ContextKey);
        return string.IsNullOrEmpty(env) ? null : new EnvContext(env);
    }
}