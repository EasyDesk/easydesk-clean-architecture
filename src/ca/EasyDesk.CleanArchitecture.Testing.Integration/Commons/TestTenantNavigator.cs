using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Commons;

public class TestTenantNavigator : ITestTenantNavigator
{
    private Option<TenantInfo> _overriddenTenant = None;

    public TenantInfo Tenant => _overriddenTenant | TenantInfo.Public;

    public bool IsMultitenancyIgnored => _overriddenTenant.IsAbsent;

    public void MoveToTenant(TenantId id)
    {
        _overriddenTenant = Some(TenantInfo.Tenant(id));
    }

    public void MoveToPublic()
    {
        _overriddenTenant = Some(TenantInfo.Public);
    }

    public void IgnoreMultitenancy()
    {
        _overriddenTenant = None;
    }
}
