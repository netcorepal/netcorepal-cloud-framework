using System.Diagnostics;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.Primitives.Diagnostics;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.Extensions
{
    public static class MediatorExtension
    {
        private static readonly DiagnosticListener _diagnosticListener =
            new(NetCorePalDiagnosticListenerNames.DiagnosticListenerName);

        private const int Max_Deep = 10;

        public static async Task DispatchDomainEventsAsync(this IMediator mediator, DbContext ctx, int deep = 0,
            CancellationToken cancellationToken = default)
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

            if (domainEvents.Count == 0)
            {
                return;
            }

            domainEntities.ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                Guid id = Guid.NewGuid();
                var eventType = domainEvent.GetType();
                var eventTypeName = eventType.FullName ?? eventType.Name;
                try
                {
                    WriteDomainEventHandlerBegin(new DomainEventHandlerBegin(id, eventTypeName, domainEvent));
                    await mediator.Publish(domainEvent, cancellationToken);
                    WriteDomainEventHandlerEnd(new DomainEventHandlerEnd(id, eventTypeName, domainEvent));
                }
                catch (Exception e)
                {
                    WriteDomainEventError(new DomainEventHandlerError(id, eventTypeName, domainEvent, e));
                    throw;
                }
            }

            await DispatchDomainEventsAsync(mediator, ctx, deep + 1, cancellationToken);
        }


        static void WriteDomainEventHandlerBegin(DomainEventHandlerBegin data)
        {
            if (_diagnosticListener.IsEnabled(NetCorePalDiagnosticListenerNames.DomainEventHandlerBegin))
            {
                _diagnosticListener.Write(NetCorePalDiagnosticListenerNames.DomainEventHandlerBegin, data);
            }
        }

        static void WriteDomainEventHandlerEnd(DomainEventHandlerEnd data)
        {
            if (_diagnosticListener.IsEnabled(NetCorePalDiagnosticListenerNames.DomainEventHandlerEnd))
            {
                _diagnosticListener.Write(NetCorePalDiagnosticListenerNames.DomainEventHandlerEnd, data);
            }
        }

        static void WriteDomainEventError(DomainEventHandlerError data)
        {
            if (_diagnosticListener.IsEnabled(NetCorePalDiagnosticListenerNames.DomainEventHandlerError))
            {
                _diagnosticListener.Write(NetCorePalDiagnosticListenerNames.DomainEventHandlerError, data);
            }
        }
    }
}