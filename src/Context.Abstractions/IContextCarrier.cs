namespace NetCorePal.Context
{
    public interface IContextCarrier
    {
        void Set(string key, string val);
    }
}
