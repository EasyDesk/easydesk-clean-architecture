using EasyDesk.CleanArchitecture.Application.Multitenancy;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Commons;

public class TestTenantNavigator : ITenantNavigator
{
    public Option<TenantInfo> ContextTenant { get; private set; } = None;

    public TenantInfo Tenant => ContextTenant | TenantInfo.Public;

    public void MoveToTenant(TenantId id)
    {
        ContextTenant = Some(TenantInfo.Tenant(id));
    }

    public void MoveToPublic()
    {
        ContextTenant = Some(TenantInfo.Public);
    }

    public void MoveToContextTenant()
    {
        ContextTenant = None;
    }
}
