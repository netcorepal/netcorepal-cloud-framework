namespace NetCorePal.Extensions.Dto;

/// <summary>
/// 分页数据模型
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class PagedData<T>
{
    /// <summary>
    /// 构造分页数据
    /// </summary>
    /// <param name="items">分页的数据</param>
    /// <param name="total">总数据条数</param>
    /// <param name="index">当前页码，从1开始</param>
    /// <param name="size">每页条数</param>
    public PagedData(IEnumerable<T> items, int total, int index, int size)
    {
        Items = items;
        Total = total;
        Index = index;
        Size = size;
    }
    /// <summary>
    /// 分页数据
    /// </summary>
    public IEnumerable<T> Items { get; private set; }
    /// <summary>
    /// 数据总数
    /// </summary>
    public int Total { get; private set; }
    /// <summary>
    /// 当前页码，从1开始
    /// </summary>
    public int Index { get; private set; }
    /// <summary>
    /// 每页数据条数
    /// </summary>
    public int Size { get; private set; }
}