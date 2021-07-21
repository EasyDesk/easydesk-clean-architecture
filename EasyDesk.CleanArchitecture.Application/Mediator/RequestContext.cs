using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Application.UserInfo;
using MediatR;

namespace EasyDesk.CleanArchitecture.Application.Mediator
{
    public class RequestContext<TRequest, TResponse> : IRequest<Response<TResponse>>
        where TRequest : RequestBase<TResponse>
    {
        public RequestContext(TRequest request, IUserInfo userInfo)
        {
            Request = request;
            UserInfo = userInfo;
        }

        public TRequest Request { get; }

        public IUserInfo UserInfo { get; }
    }
}
