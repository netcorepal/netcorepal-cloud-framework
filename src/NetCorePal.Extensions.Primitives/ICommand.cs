using MediatR;
namespace NetCorePal.Extensions.Primitives
{

    public interface IBaseCommand { }

    public interface ICommand : IRequest, IBaseCommand { }


    public interface ICommand<out TResponse> : IRequest<TResponse>, IBaseCommand { }
}
