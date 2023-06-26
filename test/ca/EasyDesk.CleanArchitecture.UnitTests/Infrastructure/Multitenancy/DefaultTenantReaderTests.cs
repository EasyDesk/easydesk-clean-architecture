using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Http;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Multitenancy;

public class DefaultTenantReaderTests
{
    private static readonly TenantId _tenantId = TenantId.New("some-tenant-id");

    private readonly DefaultTenantReader _sut = new();
    private readonly HttpContext _httpContext = new DefaultHttpContext();

    [Fact]
    public void ShouldDetectPublicTenant_FromHttpContextAccessor()
    {
        _sut.ReadFromHttpContext(_httpContext).ShouldBeEmpty();
    }

    [Fact]
    public void ShouldDetectTenant_FromHttpContextAccessor()
    {
        _httpContext.SetupTenant(_tenantId);

        _sut.ReadFromHttpContext(_httpContext).ShouldContain(_tenantId);
    }
}
