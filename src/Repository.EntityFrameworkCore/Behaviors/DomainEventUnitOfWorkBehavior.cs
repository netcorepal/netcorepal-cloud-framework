using MediatR;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore
{

    internal class DomainEventUnitOfWorkBehavior<TDomanEvent, TResponse> : IPipelineBehavior<TDomanEvent, TResponse> where TDomanEvent : IDomainEvent
    {
        private readonly ITransactionUnitOfWork _unitOfWork;
        public DomainEventUnitOfWorkBehavior(ITransactionUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<TResponse> Handle(TDomanEvent request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_unitOfWork.CurrentTransaction != null)
            {
                var response = await next();
                await _unitOfWork.SaveEntitiesAsync(cancellationToken);
                return response;
            }


            using (var transaction = _unitOfWork.BeginTransaction())
            {
                var response = await next();
                await _unitOfWork.SaveEntitiesAsync(cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
                return response;
            }
        }
    }


}
