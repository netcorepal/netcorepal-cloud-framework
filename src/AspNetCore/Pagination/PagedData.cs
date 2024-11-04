using NetCorePal.Extensions.Primitives;

namespace NetCorePal.Extensions.AspNetCore.Pagination;

public class PagedData<T>(IEnumerable<T> items, int total, int index, int size)
{
    public IEnumerable<T> Items { get; set; } = items;
    public int Total { get; set; } = total;
    public int Index { get; set; } = index;
    public int Size { get; set; } = size;
}