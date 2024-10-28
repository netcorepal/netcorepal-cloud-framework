using MediatR;

namespace NetCorePal.Web.HostedServices;

/// <summary>
/// 
/// </summary>
/// <param name="serviceProvider"></param>
public class CreateOrderCommandBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var distributedLock = scope.ServiceProvider
                .GetRequiredService<NetCorePal.Extensions.DistributedLocks.IDistributedLock>();
            var handler = await distributedLock.TryAcquireAsync("CreateOrderCommandBackgroundService",
                TimeSpan.FromSeconds(10), stoppingToken);
            if (handler != null)
            {
                await using (handler)
                {
                    var command = new CreateOrderCommand("abc", 20, 20);
                    await mediator.Send(command, stoppingToken);
                    await Task.Delay(100, stoppingToken);
                }
            }
        }
    }
}