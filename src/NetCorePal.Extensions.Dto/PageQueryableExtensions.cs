namespace NetCorePal.Extensions.Dto;

public static class PageQueryableExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="countTotal"></param>
    /// <returns></returns>
    public static PagedData<T> ToPagedData<T>(
        this IQueryable<T> query,
        int pageIndex = 1,
        int pageSize = 10,
        bool countTotal = false)
    {
        if (pageIndex <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex), "页码必须大于 0");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "每页条数必须大于 0");
        }


        // isTotalNeeded为true时才查询总数。默认不需要总数
        var totalCount = countTotal ? query.Count() : 0;

        var items = query.Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedData<T>(items, totalCount, pageIndex, pageSize);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageRequest"></param>
    /// <returns></returns>
    public static PagedData<T> ToPagedData<T>(
        this IQueryable<T> query, IPageRequest pageRequest)
    {
        return query.ToPagedData(pageRequest.PageIndex, pageRequest.PageSize, pageRequest.CountTotal);
    }
}