using DotNetCore.CAP.Filter;
using Microsoft.Extensions.Options;

namespace NetCorePal.Extensions.DistributedTransactions.CAP;

public class EnvCapSubscribeFilter : SubscribeFilter
{
    readonly List<IIngSubscribeFilter> _filters;
    public EnvCapSubscribeFilter(IEnumerable<IIngSubscribeFilter> filters)
    {
        _filters = filters.OrderBy(p => p.Order).ToList();
    }

    public override Task OnSubscribeExecutingAsync(ExecutingContext context)
    {
        var env = string.Empty;


        return Task.CompletedTask;
    }

    public override Task OnSubscribeExecutedAsync(ExecutedContext context)
    {
        // 订阅方法执行后
        return Task.CompletedTask;
    }

    public override Task OnSubscribeExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is SkipMessageException)
        {
            context.ExceptionHandled = true;
        }

        return Task.CompletedTask;
    }



#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public sealed class SkipMessageException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    {
    }
}