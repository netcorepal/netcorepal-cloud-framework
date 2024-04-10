using MediatR;

namespace NetCorePal.Web.HostedServices;

public class CreateOrderCommandBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new CreateOrderCommand("abc", 10, 20);
        await mediator.Send(command, stoppingToken);
    }
}