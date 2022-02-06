using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using EasyDesk.Tools.Options;
using NSubstitute;
using Shouldly;
using System.Threading.Tasks;
using Xunit;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;
using static EasyDesk.Tools.Options.OptionImports;
using Next = MediatR.RequestHandlerDelegate<EasyDesk.CleanArchitecture.Application.Responses.Response<EasyDesk.Tools.Nothing>>;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Mediator.Behaviors;

public class AuthorizationBehaviorTests
{
    public record TestRequest : RequestBase<Nothing>;

    [AllowUnknownUser]
    public record TestRequestWithUnknownUserAllowed : RequestBase<Nothing>;

    private readonly UserInfo _userInfo = new("user");
    private readonly IUserInfoProvider _userInfoProvider;
    private readonly IAuthorizer<TestRequest> _authorizer;
    private readonly TestRequest _request = new();
    private readonly Next _next;

    public AuthorizationBehaviorTests()
    {
        _userInfoProvider = Substitute.For<IUserInfoProvider>();
        _userInfoProvider.GetUserInfo().Returns(None);

        _next = Substitute.For<Next>();
        _next().Returns(Ok);

        _authorizer = Substitute.For<IAuthorizer<TestRequest>>();
        _authorizer.IsAuthorized(_request, _userInfo).Returns(false);
    }

    private async Task ShouldNotBeAuthorized<T>(Error error, bool authorizerResult = false) where T : RequestBase<Nothing>, new()
    {
        var result = await Handle<T>(authorizerResult);
        result.ShouldBe(error);
        await _next.DidNotReceive()();
    }

    private async Task ShouldBeAuthorized<T>(bool authorizerResult = false) where T : RequestBase<Nothing>, new()
    {
        var result = await Handle<T>(authorizerResult);
        result.ShouldBe(Ok);
        await _next.Received(1)();
    }

    private async Task<Response<Nothing>> Handle<T>(bool authorizerResult) where T : RequestBase<Nothing>, new()
    {
        var request = new T();
        var authorizer = Substitute.For<IAuthorizer<T>>();
        authorizer.IsAuthorized(request, _userInfo).Returns(authorizerResult);
        var behavior = new AuthorizationBehavior<T, Nothing>(authorizer, _userInfoProvider);
        return await behavior.Handle(request, default, _next);
    }

    private void Authenticate() => _userInfoProvider.GetUserInfo().Returns(_userInfo.AsSome());

    [Fact]
    public async Task ShouldAllowNonAuthenticatedUserIfRequestAllowsUnknownUser()
    {
        await ShouldBeAuthorized<TestRequestWithUnknownUserAllowed>();
    }

    [Fact]
    public async Task ShouldAllowAuthenticatedUserIfRequestAllowsUnknownUser()
    {
        Authenticate();
        await ShouldBeAuthorized<TestRequestWithUnknownUserAllowed>(authorizerResult: true);
    }

    [Fact]
    public async Task ShouldAllowAuthenticatedUserIfTheyAreAuthorized()
    {
        Authenticate();
        await ShouldBeAuthorized<TestRequest>(authorizerResult: true);
    }

    [Fact]
    public async Task ShouldNotAllowNonAuthenticatedUserIfTheRequestDoesNotAllowUnknownUsers()
    {
        await ShouldNotBeAuthorized<TestRequest>(Errors.UnknownUser());
    }

    [Fact]
    public async Task ShouldNotAllowAuthenticatedUserIfTheyAreNotAuthorized()
    {
        Authenticate();
        await ShouldNotBeAuthorized<TestRequest>(Errors.Forbidden());
    }
}
