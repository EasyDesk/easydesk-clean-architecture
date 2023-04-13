using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Context;

public class ContextProviderTests
{
    private const string UserId = "some-user-id";

    private readonly BasicContextProvider _sut;
    private readonly HttpContext _httpContext = new DefaultHttpContext();
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ContextProviderOptions _options = new();

    public ContextProviderTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContextAccessor.HttpContext.Returns(_httpContext);

        _sut = new BasicContextProvider(_httpContextAccessor, _options);
    }

    [Fact]
    public void ShouldDetectAuthentication_FromHttpContextAccessor()
    {
        _httpContext.SetupAuthenticatedUser(UserId);

        _sut.User.IsPresent.ShouldBeTrue();
        _sut.Context.ShouldBe(new AuthenticatedRequestContext(UserInfo.Create(UserId)));
    }
}
