namespace NetCorePal.Extensions.AspNetCore.Pagination;

public class PagedData<T>(IEnumerable<T> items, int total, int page, int size)
{
    public IEnumerable<T> Items { get; set; } = items;
    public int Total { get; set; } = total;
    public int Index { get; set; } = page;
    public int Size { get; set; } = size;
}
