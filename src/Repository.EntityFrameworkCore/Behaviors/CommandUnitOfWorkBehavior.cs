using MediatR;
using NetCorePal.Extensions.Primitives;
using System.Diagnostics;
using NetCorePal.Extensions.Primitives.Diagnostics;

namespace NetCorePal.Extensions.Repository.EntityFrameworkCore.Behaviors
{
    internal class CommandUnitOfWorkBehavior<TCommand, TResponse> : IPipelineBehavior<TCommand, TResponse>
        where TCommand : IBaseCommand
    {
#pragma warning disable S2743
        private static readonly DiagnosticListener _diagnosticListener =
#pragma warning restore S2743
            new DiagnosticListener(NetCorePalDiagnosticListenerNames.DiagnosticListenerName);

        private readonly ITransactionUnitOfWork _unitOfWork;

        public CommandUnitOfWorkBehavior(ITransactionUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<TResponse> Handle(TCommand request, RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var cmdType = typeof(TCommand);
            var commandName = cmdType.FullName ?? cmdType.Name;
            Guid id = Guid.NewGuid();
            if (_unitOfWork.CurrentTransaction != null)
            {
                try
                {
                    WriteCommandBegin(new CommandBegin(id, commandName, request));
                    var response = await next();
                    WriteCommandEnd(new CommandEnd(id, commandName, request));
                    await _unitOfWork.SaveEntitiesAsync(cancellationToken);
                    return response;
                }
                catch (Exception e)
                {
                    WriteCommandError(new CommandError(id, commandName, request, e));
                    throw;
                }
            }


            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            // 此处为了支持 async 开启事务，同时使得CAP组件正常工作，所以需要手动设置CurrentTransaction，以确保事务提交之前，CAP事件不会被发出
            // see: https://github.com/dotnetcore/CAP/issues/1656
            _unitOfWork.CurrentTransaction = transaction; 
            await using (_unitOfWork.CurrentTransaction!)
            {
                try
                {
                    WriteCommandBegin(new CommandBegin(id, commandName, request));
                    var response = await next();
                    WriteCommandEnd(new CommandEnd(id, commandName, request));
                    await _unitOfWork.SaveEntitiesAsync(cancellationToken);
                    await _unitOfWork.CommitAsync(cancellationToken);
                    return response;
                }
                catch (Exception e)
                {
                    WriteCommandError(new CommandError(id, commandName, request, e));
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }


        void WriteCommandBegin(CommandBegin data)
        {
            if (_diagnosticListener.IsEnabled(NetCorePalDiagnosticListenerNames.CommandHandlerBegin))
            {
                _diagnosticListener.Write(NetCorePalDiagnosticListenerNames.CommandHandlerBegin, data);
            }
        }

        void WriteCommandEnd(CommandEnd data)
        {
            if (_diagnosticListener.IsEnabled(NetCorePalDiagnosticListenerNames.CommandHandlerEnd))
            {
                _diagnosticListener.Write(NetCorePalDiagnosticListenerNames.CommandHandlerEnd, data);
            }
        }

        void WriteCommandError(CommandError data)
        {
            if (_diagnosticListener.IsEnabled(NetCorePalDiagnosticListenerNames.CommandHandlerError))
            {
                _diagnosticListener.Write(NetCorePalDiagnosticListenerNames.CommandHandlerError, data);
            }
        }
    }
}