using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Multitenancy;

public class DefaultContextTenantReaderTests
{
    private static readonly TenantId _tenantId = TenantId.New("some-tenant-id");

    private readonly DefaultContextTenantReader _sut;
    private readonly IContextProvider _contextProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpContext _httpContext = new DefaultHttpContext();

    public DefaultContextTenantReaderTests()
    {
        _contextProvider = Substitute.For<IContextProvider>();
        _contextProvider.Context.Returns(new AnonymousRequestContext());

        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContextAccessor.HttpContext.Returns(_httpContext);

        _sut = new(_contextProvider, _httpContextAccessor);
    }

    [Fact]
    public void ShouldDetectPublicTenant_FromHttpContextAccessor()
    {
        _sut.GetTenantId().ShouldBeEmpty();
    }

    [Fact]
    public void ShouldDetectTenant_FromHttpContextAccessor()
    {
        _httpContext.SetupTenant(_tenantId);

        _sut.GetTenantId().ShouldContain(_tenantId.Value);
    }
}
