using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Context;

public class ContextProviderTests
{
    private static readonly Agent _agent = Agent.FromSingleIdentity(Realm.Default, IdentityId.New("some-id"));

    private readonly BasicContextProvider _sut;
    private readonly HttpContext _httpContext = new DefaultHttpContext();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClaimsPrincipalParser<Agent> _agentParser = ClaimsPrincipalParsers.DefaultAgentParser();

    public ContextProviderTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContextAccessor.HttpContext.Returns(_httpContext);

        _sut = new BasicContextProvider(_httpContextAccessor, _agentParser);
    }

    [Fact]
    public void ShouldDetectAuthentication_FromHttpContextAccessor()
    {
        _httpContext.SetupAuthenticatedAgent(_agent);

        _sut.CurrentContext.ShouldBe(new ContextInfo.AuthenticatedRequest(_agent));
    }
}
