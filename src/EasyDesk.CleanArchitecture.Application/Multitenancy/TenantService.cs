namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

internal class TenantService : ITenantNavigator, IContextTenantInitializer
{
    private Option<TenantInfo> _contextTenantInfo = None;
    private Func<TenantInfo, TenantInfo> _tenantInfoOverride = t => t;

    public void Initialize(TenantInfo tenantInfo)
    {
        if (_contextTenantInfo.IsPresent)
        {
            throw new InvalidOperationException("Trying to initialize tenant after it was already initialized");
        }
        _contextTenantInfo = Some(tenantInfo);
    }

    public void MoveToTenant(TenantId id) => _tenantInfoOverride = _ => TenantInfo.Tenant(id);

    public void MoveToPublic() => _tenantInfoOverride = _ => TenantInfo.Public;

    public void BackToContextTenant() => _tenantInfoOverride = t => t;

    public TenantInfo TenantInfo => _contextTenantInfo
        .Map(_tenantInfoOverride)
        .OrElseThrow(() => new InvalidOperationException("Accessing tenant before initialization"));
}
