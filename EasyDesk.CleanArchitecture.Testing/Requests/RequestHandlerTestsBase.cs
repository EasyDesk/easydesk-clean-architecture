using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using System.Threading.Tasks;

namespace EasyDesk.Testing.Utils.Requests
{
    public abstract class RequestHandlerTestsBase<THandler, TRequest, TResponse>
        where THandler : RequestHandlerBase<TRequest, TResponse>
        where TRequest : RequestBase<TResponse>
    {
        protected abstract THandler CreateHandler();

        protected Task<Response<TResponse>> Send(TRequest request) => CreateHandler().Handle(request, default);
    }
}
