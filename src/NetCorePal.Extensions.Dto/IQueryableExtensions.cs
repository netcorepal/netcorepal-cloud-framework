using System.Linq.Expressions;

namespace NetCorePal.Extensions.Dto;

/// <summary>
/// 
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// 根据条件决定是否使用表达式进行where查询
    /// </summary>
    /// <param name="source">原始查询集合</param>
    /// <param name="condition">如果为true，则使用where表达式，否则不使用表达式</param>
    /// <param name="predicate">用于where的条件表达式</param>
    /// <typeparam name="T">The type of the data in the data source</typeparam>
    /// <returns>如果condition为true，则返回使用了表达式predicate的where结果；否则，返回原source</returns>
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        if (condition)
        {
            return source.Where(predicate);
        }

        return source;
    }
}