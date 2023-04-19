namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class TenantService : ITenantNavigator, IContextTenantInitializer
{
    private Option<TenantInfo> _contextTenantInfo = None;
    private Option<TenantInfo> _overriddenTenantInfo = None;

    public void Initialize(TenantInfo tenantInfo)
    {
        if (_contextTenantInfo.IsPresent)
        {
            throw new InvalidOperationException("Trying to initialize tenant after it was already initialized");
        }

        _contextTenantInfo = Some(tenantInfo);
    }

    public void MoveToTenant(TenantId id) => MoveToTenantInfo(Some(TenantInfo.Tenant(id)));

    public void MoveToPublic() => MoveToTenantInfo(Some(TenantInfo.Public));

    public void MoveToContextTenant() => MoveToTenantInfo(None);

    private void MoveToTenantInfo(Option<TenantInfo> tenantInfo)
    {
        if (_contextTenantInfo.IsAbsent)
        {
            throw new InvalidOperationException("Trying to move to a different tenant before initialization");
        }
        _overriddenTenantInfo = tenantInfo;
    }

    public TenantInfo TenantInfo => _overriddenTenantInfo.OrElse(
        _contextTenantInfo.OrElseThrow(() => new InvalidOperationException("Accessing tenant before initialization")));
}
