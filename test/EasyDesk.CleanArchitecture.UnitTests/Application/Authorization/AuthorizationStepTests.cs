using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Authorization;

public class AuthorizationStepTests
{
    public record TestRequest : ICqrsRequest<Nothing>;

    [AllowUnknownUser]
    public record TestRequestWithUnknownUserAllowed : ICqrsRequest<Nothing>;

    private readonly UserInfo _userInfo = new("user");
    private readonly IUserInfoProvider _userInfoProvider;
    private readonly IAuthorizer<TestRequest> _authorizer;
    private readonly TestRequest _request = new();
    private readonly NextPipelineStep<Nothing> _next;

    public AuthorizationStepTests()
    {
        _userInfoProvider = Substitute.For<IUserInfoProvider>();
        _userInfoProvider.GetUserInfo().Returns(None);

        _next = Substitute.For<NextPipelineStep<Nothing>>();
        _next().Returns(Ok);

        _authorizer = Substitute.For<IAuthorizer<TestRequest>>();
        _authorizer.IsAuthorized(_request, _userInfo).Returns(false);
    }

    private async Task ShouldNotBeAuthorized<T>(Error error, bool authorizerResult = false) where T : ICqrsRequest<Nothing>, new()
    {
        var result = await Handle<T>(authorizerResult);
        result.ShouldBe(error);
        await _next.DidNotReceive()();
    }

    private async Task ShouldBeAuthorized<T>(bool authorizerResult = false) where T : ICqrsRequest<Nothing>, new()
    {
        var result = await Handle<T>(authorizerResult);
        result.ShouldBe(Ok);
        await _next.Received(1)();
    }

    private async Task<Result<Nothing>> Handle<T>(bool authorizerResult) where T : ICqrsRequest<Nothing>, new()
    {
        var request = new T();
        var authorizer = Substitute.For<IAuthorizer<T>>();
        authorizer.IsAuthorized(request, _userInfo).Returns(authorizerResult);
        var step = new AuthorizationStep<T, Nothing>(authorizer, _userInfoProvider);
        return await step.Run(request, _next);
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
