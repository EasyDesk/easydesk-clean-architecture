using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
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
    public record TestRequest : IDispatchable<Nothing>, IRequest;

    private readonly Agent _agent = Agent.FromSingleIdentity(Realm.Default, new IdentityId("identity"));
    private readonly IAuthorizationProvider _authorizationProvider;
    private readonly NextPipelineStep<Nothing> _next;

    public StaticAuthorizationStepTests()
    {
        _next = Substitute.For<NextPipelineStep<Nothing>>();
        _next().Returns(Ok);

        _authorizationProvider = Substitute.For<IAuthorizationProvider>();
        _authorizationProvider.GetAuthorizationInfo().Returns(None);
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
        var step = new StaticAuthorizationStep<T, Nothing>(new[] { authorizer }, _authorizationProvider);
        return await step.Run(request, _next);
    }

    private void Authenticate() => _authorizationProvider.GetAuthorizationInfo().Returns(Some(ToAuthorizationInfo(_agent)));

    [Fact]
    public async Task ShouldAllowAuthenticatedIdentityIfTheyAreAuthorized()
    {
        Authenticate();
        await ShouldBeAuthorized<TestRequest>(authorizerResult: true);
    }

    [Fact]
    public async Task ShouldAllowNonAuthenticatedIdentity()
    {
        await ShouldBeAuthorized<TestRequest>(authorizerResult: true);
    }

    [Fact]
    public async Task ShouldNotAllowAuthenticatedIdentityIfTheyAreNotAuthorized()
    {
        Authenticate();
        await ShouldNotBeAuthorized<TestRequest>(Errors.Forbidden());
    }
}
