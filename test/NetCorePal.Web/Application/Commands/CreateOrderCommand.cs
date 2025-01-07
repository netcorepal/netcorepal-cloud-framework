using FluentValidation;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Web.Domain;

namespace NetCorePal.Web.Application.Commands;

/// <summary>
/// 
/// </summary>
/// <param name="Name"></param>
/// <param name="Price"></param>
/// <param name="Count"></param>
public record CreateOrderCommand(string Name, int Price, int Count) : ICommand<OrderId>;

public class CreateOrderCommandLock : ICommandLock<CreateOrderCommand>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<CommandLockSettings> GetLockKeysAsync(CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CommandLockSettings(command.Name));
    }
}

/// <summary>
/// 
/// </summary>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    /// <summary>
    /// 
    /// </summary>
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Price).InclusiveBetween(18, 60);
        RuleFor(x => x.Count).MustAsync(async (c, ct) =>
        {
            await Task.CompletedTask;
            return c > 0;
        });
    }
}

/// <summary>
/// 
/// </summary>
/// <param name="orderRepository"></param>
/// <param name="logger"></param>
public class CreateOrderCommandHandler(IOrderRepository orderRepository, ILogger<CreateOrderCommandHandler> logger)
    : ICommandHandler<CreateOrderCommand, OrderId>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OrderId> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var a = new List<long>();
        for (int i = 0; i < 1000; i++)
        {
            a.Add(i);
        }

        await Parallel.ForEachAsync(a, new ParallelOptions(), async (item, c) => { await Task.Delay(12, c); });

        var order = new Order(request.Name, request.Count);
        order = await orderRepository.AddAsync(order, cancellationToken);
        logger.LogInformation("order created, id:{orderId}", order.Id);
        return order.Id;
    }
}