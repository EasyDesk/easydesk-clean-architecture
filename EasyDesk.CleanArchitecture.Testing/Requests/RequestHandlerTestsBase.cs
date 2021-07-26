using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Application.UserInfo;
using NSubstitute;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Testing.Requests
{
    public abstract class RequestHandlerTestsBase<THandler, TRequest, TResponse>
        where THandler : RequestHandlerBase<TRequest, TResponse>
        where TRequest : RequestBase<TResponse>
    {
        public RequestHandlerTestsBase() : this(Substitute.For<IUserInfo>())
        {
            UserInfo.IsLoggedIn.Returns(false);
        }

        public RequestHandlerTestsBase(IUserInfo userInfo)
        {
            UserInfo = userInfo;
        }

        protected IUserInfo UserInfo { get; }

        protected abstract THandler CreateHandler();

        protected Task<Response<TResponse>> Send(TRequest request) => CreateHandler().Handle(new RequestContext<TRequest, TResponse>(request, UserInfo), default);
    }
}
