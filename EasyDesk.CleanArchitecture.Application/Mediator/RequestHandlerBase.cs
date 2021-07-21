using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Application.UserInfo;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Mediator
{
    public abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler<RequestContext<TRequest, TResponse>, Response<TResponse>>
        where TRequest : RequestBase<TResponse>
    {
        public async Task<Response<TResponse>> Handle(RequestContext<TRequest, TResponse> context, CancellationToken cancellationToken)
        {
            if (!IsAuthorized(context.Request, context.UserInfo))
            {
                return Errors.Forbidden();
            }

            return await Handle(context.Request);
        }

        protected virtual bool IsAuthorized(TRequest request, IUserInfo userInfo) => true;

        protected abstract Task<Response<TResponse>> Handle(TRequest request);
    }
}
