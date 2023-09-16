namespace NetCorePal.Extensions.Repository.EntityFrameworkCore
{
    /// <summary>
    /// 
    /// </summary>
    public class RecursionOverflowException : Exception
    {
        public int MaxDeep { get; set; }

        public RecursionOverflowException(int maxDeep, string error) : base(error)
        {
            MaxDeep = maxDeep;
        }
    }
}
