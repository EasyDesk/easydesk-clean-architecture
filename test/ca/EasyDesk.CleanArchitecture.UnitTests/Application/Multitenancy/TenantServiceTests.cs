using EasyDesk.CleanArchitecture.Application.Multitenancy;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Multitenancy;

public class TenantServiceTests
{
    private readonly TenantInfo _tenantInfo;
    private readonly TenantId _tenantId;
    private readonly TenantService _sut;

    public TenantServiceTests()
    {
        _sut = new();
        _tenantId = new("test-tenant-id");
        _tenantInfo = TenantInfo.Tenant(_tenantId);
    }

    [Fact]
    public void ShouldThrow_IfInitializedMoreThanOnce()
    {
        _sut.Initialize(_tenantInfo);
        Should.Throw<InvalidOperationException>(() => _sut.Initialize(_tenantInfo));
        Should.Throw<InvalidOperationException>(() => _sut.Initialize(_tenantInfo));
    }

    [Fact]
    public void NavigateTo_ShouldThrow_IfUsedWithoutInitialization()
    {
        Should.Throw<InvalidOperationException>(() => _sut.NavigateTo(_tenantInfo));
    }

    [Fact]
    public void TenantInfo_ShouldThrow_IfUsedWithoutInitialization()
    {
        Should.Throw<InvalidOperationException>(() => _sut.Tenant);
    }

    [Fact]
    public void TenantInfo_ShouldReturnTheCorrectTenantInfoAfterInitialization_WithTenant()
    {
        _sut.Initialize(_tenantInfo);
        _sut.Tenant.ShouldBe(_tenantInfo);
    }

    [Fact]
    public void TenantInfo_ShouldReturnTheCorrectTenantInfoAfterInitialization_WithPublic()
    {
        _sut.Initialize(TenantInfo.Public);
        _sut.Tenant.ShouldBe(TenantInfo.Public);
    }

    [Fact]
    public void TenantInfo_ShouldBeMutualExclusiveWithTenantAfterMove()
    {
        _sut.Initialize(TenantInfo.Public);
        _sut.NavigateToTenant(_tenantId);
        _sut.Tenant.ShouldBe(_tenantInfo);
    }

    [Fact]
    public void TenantInfo_ShouldBeMutualExclusiveWithPublicAfterMove()
    {
        _sut.Initialize(_tenantInfo);
        _sut.NavigateToPublic();
        _sut.Tenant.ShouldBe(TenantInfo.Public);
    }
}
