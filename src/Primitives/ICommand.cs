using MediatR;
namespace NetCorePal.Extensions.Primitives
{
    public interface ICommand : IRequest { }


    public interface ICommand<out TResponse> : IRequest<TResponse>, ICommand { }
}
