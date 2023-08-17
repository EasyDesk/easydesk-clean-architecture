namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class TenantService : ITenantNavigator, IContextTenantInitializer
{
    private Option<TenantInfo> _overriddenTenantInfo = None;

    public void Initialize(TenantInfo tenantInfo)
    {
        if (ContextTenant.IsPresent)
        {
            throw new InvalidOperationException("Trying to initialize tenant after it was already initialized");
        }

        ContextTenant = Some(tenantInfo);
    }

    public Option<TenantInfo> ContextTenant { get; private set; } = None;

    public TenantInfo Tenant => _overriddenTenantInfo.OrElse(
        ContextTenant.OrElseThrow(() => new InvalidOperationException("Accessing tenant before initialization")));

    public void MoveToTenant(TenantId id) => MoveToTenantInfo(Some(TenantInfo.Tenant(id)));

    public void MoveToPublic() => MoveToTenantInfo(Some(TenantInfo.Public));

    public void MoveToContextTenant() => MoveToTenantInfo(None);

    private void MoveToTenantInfo(Option<TenantInfo> tenantInfo)
    {
        if (ContextTenant.IsAbsent)
        {
            throw new InvalidOperationException("Trying to move to a different tenant before initialization");
        }
        _overriddenTenantInfo = tenantInfo;
    }
}
