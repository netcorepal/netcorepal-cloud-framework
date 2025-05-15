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

    /// <summary>
    /// 根据条件决定是否使用表达式进行 OrderBy 查询
    /// </summary>
    /// <param name="source">原始查询集合</param>
    /// <param name="condition">如果为 true，则使用 OrderBy 表达式，否则不使用表达式</param>
    /// <param name="predicate">用于 OrderBy 的条件表达式</param>
    /// <param name="desc">是否倒序排序，默认 false</param>
    /// <typeparam name="TSource">数据源类型</typeparam>
    /// <typeparam name="TKey">排序属性类型</typeparam>
    /// <returns>如果 condition 为 true，则返回使用了表达式 predicate 的 OrderBy 的后的 IOrderedQueryable，否则返回不影响排序的 IOrderedQueryable 集合</returns>
    public static IOrderedQueryable<TSource> OrderByIf<TSource, TKey>(
        this IQueryable<TSource> source,
        bool condition,
        Expression<Func<TSource, TKey>> predicate,
        bool desc = false
    )
    {
        if (condition)
        {
            return desc ? source.OrderByDescending<TSource, TKey>(predicate) : source.OrderBy<TSource, TKey>(predicate);
        }

        return source.OrderBy(data => 0);
    }

    /// <summary>
    /// 根据条件决定是否使用表达式进行 ThenBy 查询
    /// </summary>
    /// <param name="source">原始查询集合</param>
    /// <param name="condition">如果为 true，则使用 ThenBy 表达式，否则不使用表达式</param>
    /// <param name="predicate">用于 ThenBy 的条件表达式</param>
    /// <param name="desc">是否倒序排序，默认 false</param>
    /// <typeparam name="TSource">数据源类型</typeparam>
    /// <typeparam name="TKey">排序属性类型</typeparam>
    /// <returns>如果 condition 为 true，则返回使用了表达式 predicate 的 ThenBy 的后的结果，否则返回源 Source 集合</returns>
    public static IOrderedQueryable<TSource> ThenByIf<TSource, TKey>(
        this IOrderedQueryable<TSource> source,
        bool condition,
        Expression<Func<TSource, TKey>> predicate,
        bool desc = false
    )
    {
        if (condition)
        {
            return desc ? source.ThenByDescending<TSource, TKey>(predicate) : source.ThenBy<TSource, TKey>(predicate);
        }

        return source;
    }
}