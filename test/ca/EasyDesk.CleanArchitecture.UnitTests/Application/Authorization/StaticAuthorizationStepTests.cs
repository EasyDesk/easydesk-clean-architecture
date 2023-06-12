using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using NSubstitute;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Authorization;

public class StaticAuthorizationStepTests
{
    public record TestRequest : IDispatchable<Nothing>;

    [AllowUnknownUser]
    public record TestRequestWithUnknownUserAllowed : IDispatchable<Nothing>;

    private readonly UserInfo _userInfo = new(UserId.New("user"));
    private readonly IContextProvider _contextProvider;
    private readonly IStaticAuthorizer _authorizer;
    private readonly TestRequest _request = new();
    private readonly NextPipelineStep<Nothing> _next;

    public StaticAuthorizationStepTests()
    {
        _contextProvider = Substitute.For<IContextProvider>();
        _contextProvider.CurrentContext.Returns(new ContextInfo.AnonymousRequest());

        _next = Substitute.For<NextPipelineStep<Nothing>>();
        _next().Returns(Ok);

        _authorizer = Substitute.For<IStaticAuthorizer>();
        _authorizer.IsAuthorized(_request, _userInfo).Returns(false);
    }

    private async Task ShouldNotBeAuthorized<T>(Error error, bool authorizerResult = false) where T : IDispatchable<Nothing>, new()
    {
        var result = await Handle<T>(authorizerResult);
        result.ShouldBe(error);
        await _next.DidNotReceive()();
    }

    private async Task ShouldBeAuthorized<T>(bool authorizerResult = false) where T : IDispatchable<Nothing>, new()
    {
        var result = await Handle<T>(authorizerResult);
        result.ShouldBe(Ok);
        await _next.Received(1)();
    }

    private async Task<Result<Nothing>> Handle<T>(bool authorizerResult) where T : IDispatchable<Nothing>, new()
    {
        var request = new T();
        var authorizer = Substitute.For<IStaticAuthorizer>();
        authorizer.IsAuthorized(request, _userInfo).Returns(authorizerResult);
        var step = new StaticAuthorizationStep<T, Nothing>(authorizer, _contextProvider);
        return await step.Run(request, _next);
    }

    private void Authenticate() => _contextProvider.CurrentContext.Returns(new ContextInfo.AuthenticatedRequest(_userInfo));

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
        await ShouldNotBeAuthorized<TestRequest>(new UnknownUserError());
    }

    [Fact]
    public async Task ShouldNotAllowAuthenticatedUserIfTheyAreNotAuthorized()
    {
        Authenticate();
        await ShouldNotBeAuthorized<TestRequest>(Errors.Forbidden());
    }
}
