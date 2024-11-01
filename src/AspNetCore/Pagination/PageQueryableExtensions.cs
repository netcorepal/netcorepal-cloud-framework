using Microsoft.EntityFrameworkCore;

namespace NetCorePal.Extensions.AspNetCore.Pagination;

public static class PageQueryableExtensions
{
    public static async Task<PagedData<T>> ToPagedDataAsync<T>(
        this IQueryable<T> query,
        int? index,
        int? size,
        bool? countTotal,
        CancellationToken cancellationToken)
    {
        var pageIndex = index ?? 1; // 默认取第1页
        var pageSize = size ?? 10; // 默认每页10条

        // isTotalNeeded为true时才查询总数。默认不需要总数
        var totalCount = (countTotal ?? false )? await query.CountAsync(cancellationToken) : 0; 

        var items = await query.Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedData<T>(items, totalCount, pageIndex, pageSize);
    }
}
