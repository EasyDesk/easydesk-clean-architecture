using EasyDesk.CleanArchitecture.Application.Multitenancy;
using NSubstitute;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Multitenancy;

public class MultiTenantPoliciesTests
{
    private static readonly TenantId _tenantId = TenantId.New("test-tenant-id");
    private static readonly TenantId _tenantId2 = TenantId.New("test-tenant-id2");
    private static readonly TenantId _nonExistentId = TenantId.New("test-tenant-id-nope");
    private readonly IMultitenancyManager _multitenancyManager;

    public MultiTenantPoliciesTests()
    {
        _multitenancyManager = Substitute.For<IMultitenancyManager>();
        _multitenancyManager.TenantExists(_tenantId).Returns(Task.FromResult(true));
        _multitenancyManager.TenantExists(_tenantId2).Returns(Task.FromResult(true));
        _multitenancyManager.TenantExists(_nonExistentId).Returns(Task.FromResult(false));
    }

    public static IEnumerable<object[]> Tenants()
    {
        yield return new object[] { TenantInfo.Tenant(_tenantId) };
        yield return new object[] { TenantInfo.Tenant(_tenantId2) };
        yield return new object[] { TenantInfo.Public };
        yield return new object[] { TenantInfo.Tenant(_nonExistentId) };
    }

    [Theory]
    [MemberData(nameof(Tenants))]
    public async Task IgnoreAndUsePublic(TenantInfo tenantInfo)
    {
        var sut = MultitenantPolicies.IgnoreAndUsePublic();
        var info = await sut(tenantInfo, _multitenancyManager);
        info.IsSuccess.ShouldBeTrue();
        info.ReadValue().ShouldBe(TenantInfo.Public);
    }

    [Theory]
    [MemberData(nameof(Tenants))]
    public async Task IgnoreAndUseTenant(TenantInfo tenantInfo)
    {
        var sut = MultitenantPolicies.IgnoreAndUseTenant(_nonExistentId);
        var info = await sut(tenantInfo, _multitenancyManager);
        info.IsSuccess.ShouldBeTrue();
        info.ReadValue().RequireId().ShouldBe(_nonExistentId);
    }

    [Theory]
    [MemberData(nameof(Tenants))]
    public async Task IgnoreAndUseExistingTenant(TenantInfo tenantInfo)
    {
        var sut = MultitenantPolicies.IgnoreAndUseExistingTenant(_tenantId);
        var info = await sut(tenantInfo, _multitenancyManager);
        info.IsSuccess.ShouldBeTrue();
        info.ReadValue().RequireId().ShouldBe(_tenantId);
    }

    [Theory]
    [MemberData(nameof(Tenants))]
    public async Task IgnoreAndUseExistingTenant_ShouldReturnFailure_WithNonexistingTenant(TenantInfo tenantInfo)
    {
        var sut = MultitenantPolicies.IgnoreAndUseExistingTenant(_nonExistentId);
        var info = await sut(tenantInfo, _multitenancyManager);
        info.IsSuccess.ShouldBeFalse();
        info.ReadError().ShouldBeOfType<TenantNotFoundError>();
    }

    [Theory]
    [MemberData(nameof(Tenants))]
    public async Task AnyTenantOrPublic(TenantInfo tenantInfo)
    {
        var sut = MultitenantPolicies.AnyTenantOrPublic();
        var info = await sut(tenantInfo, _multitenancyManager);
        info.IsSuccess.ShouldBe(true);
        info.ReadValue().ShouldBe(tenantInfo);
    }

    [Theory]
    [MemberData(nameof(Tenants))]
    public async Task RequirePublic(TenantInfo tenantInfo)
    {
        var sut = MultitenantPolicies.RequirePublic();
        var info = await sut(tenantInfo, _multitenancyManager);
        info.IsSuccess.ShouldBe(tenantInfo.IsPublic);
        if (info.IsSuccess)
        {
            info.ReadValue().ShouldBe(TenantInfo.Public);
        }
        else
        {
            info.ReadError().ShouldBeOfType<MultitenancyNotSupportedError>();
        }
    }

    [Theory]
    [MemberData(nameof(Tenants))]
    public async Task RequireAnyTenant(TenantInfo tenantInfo)
    {
        var sut = MultitenantPolicies.RequireAnyTenant();
        var info = await sut(tenantInfo, _multitenancyManager);
        info.IsSuccess.ShouldBe(tenantInfo.IsInTenant);
        if (info.IsSuccess)
        {
            info.ReadValue().ShouldBe(tenantInfo);
        }
        else
        {
            info.ReadError().ShouldBeOfType<MissingTenantError>();
        }
    }

    public static IEnumerable<object[]> TenantsAndExistence()
    {
        yield return new object[] { TenantInfo.Tenant(_tenantId), true };
        yield return new object[] { TenantInfo.Tenant(_tenantId2), true };
        yield return new object[] { TenantInfo.Public, true };
        yield return new object[] { TenantInfo.Tenant(_nonExistentId), false };
    }

    [Theory]
    [MemberData(nameof(TenantsAndExistence))]
    public async Task ExistingTenantOrPublic(TenantInfo tenantInfo, bool exists)
    {
        var sut = MultitenantPolicies.ExistingTenantOrPublic();
        var info = await sut(tenantInfo, _multitenancyManager);
        info.IsSuccess.ShouldBe(exists);
        if (info.IsSuccess)
        {
            info.ReadValue().ShouldBe(tenantInfo);
        }
        else
        {
            info.ReadError().ShouldBeOfType<TenantNotFoundError>();
        }
    }

    [Theory]
    [MemberData(nameof(TenantsAndExistence))]
    public async Task RequireExistingTenant(TenantInfo tenantInfo, bool exists)
    {
        var sut = MultitenantPolicies.RequireExistingTenant();
        var info = await sut(tenantInfo, _multitenancyManager);
        info.IsSuccess.ShouldBe(exists && tenantInfo.IsInTenant);
        if (info.IsSuccess)
        {
            info.ReadValue().ShouldBe(tenantInfo);
        }
        else if (tenantInfo.IsPublic)
        {
            info.ReadError().ShouldBeOfType<MissingTenantError>();
        }
        else
        {
            info.ReadError().ShouldBeOfType<TenantNotFoundError>();
        }
    }
}
