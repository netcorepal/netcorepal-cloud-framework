using MediatR;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.Behaviors
{
    internal class CommandUnitOfWorkBehavior<TCommand, TResponse> : IPipelineBehavior<TCommand, TResponse> where TCommand : IBaseCommand
    {
        private readonly ITransactionUnitOfWork _unitOfWork;
        public CommandUnitOfWorkBehavior(ITransactionUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<TResponse> Handle(TCommand request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
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
