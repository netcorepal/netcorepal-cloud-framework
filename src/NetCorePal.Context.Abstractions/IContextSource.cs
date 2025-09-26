namespace NetCorePal.Context
{
    public interface IContextSource
    {
        string? Get(string key);
    }
}
