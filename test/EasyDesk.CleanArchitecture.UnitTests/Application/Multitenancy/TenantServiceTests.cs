using EasyDesk.CleanArchitecture.Application.Multitenancy;
using Shouldly;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Multitenancy;

public class TenantServiceTests
{
    private readonly TenantInfo _tenantInfo;
    private readonly TenantId _tenantId;
    private readonly TenantService _sut;

    public TenantServiceTests()
    {
        _sut = new TenantService();
        _tenantId = TenantId.Create("test-tenant-id");
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
        Should.Throw<InvalidOperationException>(() => _sut.MoveToPublic());
    }

    [Fact]
    public void TenantInfo_ShouldThrow_IfUsedWithoutInitialization()
    {
        Should.Throw<InvalidOperationException>(() => _sut.TenantInfo);
    }

    [Fact]
    public void ShouldThrow_IfDisposingTheWrongScope()
    {
        _sut.Initialize(new TenantInfo(None));
        using var scope1 = _sut.MoveToTenant(_tenantId);
        using (var scope2 = _sut.MoveToPublic())
        {
            Should.Throw<InvalidOperationException>(() => scope1.Dispose());
            using var scope3 = _sut.MoveToPublic();
            Should.Throw<InvalidOperationException>(() => scope2.Dispose());
        }
    }

    [Fact]
    public void IsInTenant_And_IsPublic_ShouldBeMutualExclusiveWithTenant()
    {
        _sut.Initialize(_tenantInfo);
        _sut.TenantInfo.IsInTenant.ShouldBeTrue();
        _sut.TenantInfo.IsPublic.ShouldBeFalse();
    }

    [Fact]
    public void IsInTenant_And_IsPublic_ShouldBeMutualExclusiveWithPublic()
    {
        _sut.Initialize(new TenantInfo(None));
        _sut.TenantInfo.IsInTenant.ShouldBeFalse();
        _sut.TenantInfo.IsPublic.ShouldBeTrue();
    }

    [Fact]
    public void IsInTenant_And_IsPublic_ShouldBeMutualExclusiveWithTenantAfterMove()
    {
        _sut.Initialize(new TenantInfo(None));
        using var s = _sut.MoveToTenant(_tenantId);
        _sut.TenantInfo.IsInTenant.ShouldBeTrue();
        _sut.TenantInfo.IsPublic.ShouldBeFalse();
    }

    [Fact]
    public void IsInTenant_And_IsPublic_ShouldBeMutualExclusiveWithPublicAfterMove()
    {
        _sut.Initialize(_tenantInfo);
        using var s = _sut.MoveToPublic();
        _sut.TenantInfo.IsInTenant.ShouldBeFalse();
        _sut.TenantInfo.IsPublic.ShouldBeTrue();
    }

    [Fact]
    public void ShouldSuceed_WithUsingChain()
    {
        _sut.Initialize(_tenantInfo);
        _sut.TenantInfo.ShouldBe(_tenantInfo);
        using var scope1 = _sut.MoveToPublic();
        _sut.TenantInfo.Id.ShouldBe(None);
        using var scope2 = _sut.MoveToTenant(_tenantId);
        _sut.TenantInfo.Id.ShouldBe(Some(_tenantId));
        using var scope3 = _sut.MoveToPublic();
        _sut.TenantInfo.Id.ShouldBe(None);
    }

    [Fact]
    public void ShouldSucceed_WithParenthesisInception()
    {
        _sut.Initialize(_tenantInfo);
        _sut.TenantInfo.ShouldBe(_tenantInfo);
        using (var scope1 = _sut.MoveToPublic())
        {
            _sut.TenantInfo.Id.ShouldBe(None);
            using (var scope2 = _sut.MoveToTenant(_tenantId))
            {
                _sut.TenantInfo.Id.ShouldBe(Some(_tenantId));
                using (var scope3 = _sut.MoveToPublic())
                {
                    _sut.TenantInfo.Id.ShouldBe(None);
                }
                _sut.TenantInfo.Id.ShouldBe(Some(_tenantId));
            }
            _sut.TenantInfo.Id.ShouldBe(None);
        }
        _sut.TenantInfo.ShouldBe(_tenantInfo);
    }
}
