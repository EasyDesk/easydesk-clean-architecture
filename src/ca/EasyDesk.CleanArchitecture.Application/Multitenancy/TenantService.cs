namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class TenantService : ITenantNavigator, IContextTenantInitializer
{
    private Option<TenantInfo> _overriddenTenantInfo = None;

    public void Initialize(TenantInfo tenantInfo)
    {
        if (ContextTenantInfo.IsPresent)
        {
            throw new InvalidOperationException("Trying to initialize tenant after it was already initialized");
        }

        ContextTenantInfo = Some(tenantInfo);
    }

    public Option<TenantInfo> ContextTenantInfo { get; private set; } = None;

    public TenantInfo TenantInfo => _overriddenTenantInfo.OrElse(
        ContextTenantInfo.OrElseThrow(() => new InvalidOperationException("Accessing tenant before initialization")));

    public void MoveToTenant(TenantId id) => MoveToTenantInfo(Some(TenantInfo.Tenant(id)));

    public void MoveToPublic() => MoveToTenantInfo(Some(TenantInfo.Public));

    public void MoveToContextTenant() => MoveToTenantInfo(None);

    private void MoveToTenantInfo(Option<TenantInfo> tenantInfo)
    {
        if (ContextTenantInfo.IsAbsent)
        {
            throw new InvalidOperationException("Trying to move to a different tenant before initialization");
        }
        _overriddenTenantInfo = tenantInfo;
    }
}
