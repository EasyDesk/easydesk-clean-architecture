using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Application.UserInfo;
using EasyDesk.Testing.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Testing.Requests;

public abstract class RequestHandlerTestsBase<THandler, TRequest, TResponse> : DependencyInjectionTestBase
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

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<THandler>();
    }

    protected THandler CreateHandler() => Service<THandler>();

    protected Task<Response<TResponse>> Send(TRequest request) => CreateHandler().Handle(request, default);
}
