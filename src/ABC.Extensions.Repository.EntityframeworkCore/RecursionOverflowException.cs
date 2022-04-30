namespace ABC.Extensions.Repository.EntityframeworkCore
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
