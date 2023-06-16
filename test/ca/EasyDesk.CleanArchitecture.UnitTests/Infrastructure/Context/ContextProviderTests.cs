using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Context;

public class ContextProviderTests
{
    private const string IdentityName = "main";
    private static readonly IdentityId _identity = IdentityId.New("some-id");

    private readonly BasicContextProvider _sut;
    private readonly HttpContext _httpContext = new DefaultHttpContext();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClaimsPrincipalParser<Agent> _agentParser = ClaimsPrincipalParsers.ForDefaultAgent();

    public ContextProviderTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContextAccessor.HttpContext.Returns(_httpContext);

        _sut = new BasicContextProvider(_httpContextAccessor, _agentParser);
    }

    [Fact]
    public void ShouldDetectAuthentication_FromHttpContextAccessor()
    {
        _httpContext.SetupAuthenticatedIdentity(_identity);

        _sut.GetAgent().IsPresent.ShouldBeTrue();

        var agent = Agent.FromSingleIdentity(_identity, name: IdentityName);
        _sut.CurrentContext.ShouldBe(new ContextInfo.AuthenticatedRequest(agent));
    }
}
