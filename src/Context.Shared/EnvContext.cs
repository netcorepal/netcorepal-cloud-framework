namespace NetCorePal.Context;

public class EnvContext
{
    public static string ContextKey = "x-env";

    public EnvContext(string env)
    {
        Evn = env;
    }

    public string Evn { get; private set; }
}