using EasyDesk.CleanArchitecture.Application.Responses;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Mediator
{
    public abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler<RequestContext<TRequest, TResponse>, Response<TResponse>>
        where TRequest : RequestBase<TResponse>
    {
        public Task<Response<TResponse>> Handle(RequestContext<TRequest, TResponse> request, CancellationToken cancellationToken) =>
            Handle(request.Request);

        protected abstract Task<Response<TResponse>> Handle(TRequest request);
    }
}
