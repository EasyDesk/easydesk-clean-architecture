using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Results;
using NSubstitute;
using Shouldly;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Authorization;

public class StaticAuthorizationStepTests
{
    public record TestRequest : IDispatchable<Nothing>;

    [AllowUnknownAgent]
    public record TestRequestWithUnknownIdentityAllowed : IDispatchable<Nothing>;

    private readonly Agent _agent = Agent.FromSingleIdentity(Realm.Default, IdentityId.New("identity"));
    private readonly IContextProvider _contextProvider;
    private readonly IAuthorizationProvider _authorizationProvider;
    private readonly NextPipelineStep<Nothing> _next;

    public StaticAuthorizationStepTests()
    {
        _contextProvider = Substitute.For<IContextProvider>();
        _contextProvider.CurrentContext.Returns(new ContextInfo.AnonymousRequest());

        _next = Substitute.For<NextPipelineStep<Nothing>>();
        _next().Returns(Ok);

        _authorizationProvider = Substitute.For<IAuthorizationProvider>();
        _authorizationProvider.GetAuthorizationInfo().Returns(_ => _contextProvider.GetAgent().Map(ToAuthorizationInfo));
    }

    private AuthorizationInfo ToAuthorizationInfo(Agent agent) =>
        new(agent, Set<Permission>());

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
        var authorizer = Substitute.For<IStaticAuthorizer<T>>();
        authorizer.IsAuthorized(request, ToAuthorizationInfo(_agent)).Returns(authorizerResult);
        var step = new StaticAuthorizationStep<T, Nothing>(_contextProvider, new[] { authorizer }, _authorizationProvider);
        return await step.Run(request, _next);
    }

    private void Authenticate() => _contextProvider.CurrentContext.Returns(new ContextInfo.AuthenticatedRequest(_agent));

    [Fact]
    public async Task ShouldAllowNonAuthenticatedIdentityIfRequestAllowsUnknownIdentity()
    {
        await ShouldBeAuthorized<TestRequestWithUnknownIdentityAllowed>();
    }

    [Fact]
    public async Task ShouldAllowAuthenticatedIdentityIfRequestAllowsUnknownIdentity()
    {
        Authenticate();
        await ShouldBeAuthorized<TestRequestWithUnknownIdentityAllowed>(authorizerResult: true);
    }

    [Fact]
    public async Task ShouldAllowAuthenticatedIdentityIfTheyAreAuthorized()
    {
        Authenticate();
        await ShouldBeAuthorized<TestRequest>(authorizerResult: true);
    }

    [Fact]
    public async Task ShouldNotAllowNonAuthenticatedIdentityIfTheRequestDoesNotAllowUnknownIdentitys()
    {
        await ShouldNotBeAuthorized<TestRequest>(new UnknownAgentError());
    }

    [Fact]
    public async Task ShouldNotAllowAuthenticatedIdentityIfTheyAreNotAuthorized()
    {
        Authenticate();
        await ShouldNotBeAuthorized<TestRequest>(Errors.Forbidden());
    }
}
