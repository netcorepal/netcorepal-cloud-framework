using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Dto;

namespace NetCorePal.Extensions.AspNetCore;

public static class PageQueryableExtensions
{
    public static async Task<PagedData<T>> ToPagedDataAsync<T>(
        this IQueryable<T> query,
        int pageIndex = 1,
        int pageSize = 10,
        bool countTotal = false,
        CancellationToken cancellationToken = default)
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
        var totalCount = countTotal ? await query.CountAsync(cancellationToken) : 0;

        var items = await query.Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedData<T>(items, totalCount, pageIndex, pageSize);
    }


    public static Task<PagedData<T>> ToPagedDataAsync<T>(
        this IQueryable<T> query,
        IPagedQuery<T> pagedQuery,
        CancellationToken cancellationToken = default)
    {
        return query.ToPagedDataAsync(pagedQuery.PageIndex, pagedQuery.PageSize, pagedQuery.CountTotal, cancellationToken);
    }
}
