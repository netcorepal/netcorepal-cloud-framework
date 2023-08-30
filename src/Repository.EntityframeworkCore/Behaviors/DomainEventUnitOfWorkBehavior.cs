using MediatR;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Repository.EntityframeworkCore
{

    internal class DomainEventUnitOfWorkBehavior<TDomanEvent, TResponse> : IPipelineBehavior<TDomanEvent, TResponse> where TDomanEvent : IDomainEvent
    {
        private readonly IEFCoreUnitOfWork _unitOfWork;
        public DomainEventUnitOfWorkBehavior(IEFCoreUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<TResponse> Handle(TDomanEvent request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var response = await next();
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return response;
            }
        }
    }


}
