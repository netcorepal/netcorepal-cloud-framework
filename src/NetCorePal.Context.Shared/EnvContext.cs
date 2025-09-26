namespace NetCorePal.Context;

public class EnvContext
{
    public static string ContextKey { get; set; } = "x-env";

    public EnvContext(string env)
    {
        Env = env;
    }

    public string Env { get; private set; }
}