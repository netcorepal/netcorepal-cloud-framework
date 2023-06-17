using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.Repository.EntityframeworkCore.Extensions
{
    internal static class MediatorExtension
    {
        private const int Max_Deep = 10;
        public static async Task DispatchDomainEventsAsync(this IMediator mediator, DbContext ctx, int deep = 0)
        {
            var domainEntities = ctx.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.GetDomainEvents().Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.GetDomainEvents())
                .ToList();
            if (deep > Max_Deep)
            {
                throw new RecursionOverflowException(Max_Deep, "领域事件发布超过最大递归深度");
            }
            if (!domainEvents.Any())
            {
                return;
            }
            domainEntities.ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                await mediator.Publish(domainEvent);
            }
            await DispatchDomainEventsAsync(mediator, ctx, deep++);
        }
    }
}
