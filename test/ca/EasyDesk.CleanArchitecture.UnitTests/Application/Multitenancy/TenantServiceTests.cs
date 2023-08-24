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
        _sut = new TenantService();
        _tenantId = TenantId.New("test-tenant-id");
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
    public void MoveToTenant_ShouldThrow_IfUsedWithoutInitialization()
    {
        Should.Throw<InvalidOperationException>(() => _sut.MoveToTenant(_tenantInfo.Id.Value));
    }

    [Fact]
    public void MoveToPublic_ShouldThrow_IfUsedWithoutInitialization()
    {
        Should.Throw<InvalidOperationException>(_sut.MoveToPublic);
    }

    [Fact]
    public void MoveToContextTenant_ShouldThrow_IfUsedWithoutInitialization()
    {
        Should.Throw<InvalidOperationException>(_sut.MoveToContextTenant);
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
        _sut.MoveToTenant(_tenantId);
        _sut.Tenant.ShouldBe(_tenantInfo);
    }

    [Fact]
    public void TenantInfo_ShouldBeMutualExclusiveWithPublicAfterMove()
    {
        _sut.Initialize(_tenantInfo);
        _sut.MoveToPublic();
        _sut.Tenant.ShouldBe(TenantInfo.Public);
    }

    [Fact]
    public void ShouldSuceed_WithMoveChain()
    {
        _sut.Initialize(_tenantInfo);
        _sut.Tenant.ShouldBe(_tenantInfo);
        _sut.MoveToPublic();
        _sut.Tenant.ShouldBe(TenantInfo.Public);
        _sut.MoveToTenant(_tenantId);
        _sut.Tenant.ShouldBe(TenantInfo.Tenant(_tenantId));
        _sut.MoveToPublic();
        _sut.Tenant.ShouldBe(TenantInfo.Public);
    }

    [Fact]
    public void MoveToContextTenant_ShouldReturnToTheInitializedTenant_AfterMovingToPublic()
    {
        _sut.Initialize(_tenantInfo);
        _sut.MoveToPublic();
        _sut.MoveToContextTenant();
        _sut.Tenant.ShouldBe(_tenantInfo);
    }

    [Fact]
    public void MoveToContextTenant_ShouldReturnToTheInitializedTenant_AfterMovingToTenant()
    {
        _sut.Initialize(TenantInfo.Public);
        _sut.MoveToTenant(_tenantId);
        _sut.MoveToContextTenant();
        _sut.Tenant.ShouldBe(TenantInfo.Public);
    }
}
