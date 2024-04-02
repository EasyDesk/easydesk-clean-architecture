using EasyDesk.Commons.Options;
using EasyDesk.Commons.Scopes;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class TenantService : IContextTenantNavigator, IContextTenantInitializer
{
    private Option<ScopeManager<TenantInfo>> _scopeManager = None;

    public void Initialize(TenantInfo tenantInfo)
    {
        if (_scopeManager.IsPresent)
        {
            throw new InvalidOperationException("Trying to initialize tenant after it was already initialized.");
        }

        _scopeManager = Some(new ScopeManager<TenantInfo>(tenantInfo));
        ContextTenant = Some(tenantInfo);
    }

    public Option<TenantInfo> ContextTenant { get; private set; } = None;

    public TenantInfo Tenant => _scopeManager
        .OrElseThrow(() => new InvalidOperationException("Accessing tenant before initialization."))
        .Current;

    public IDisposable NavigateTo(TenantInfo tenantInfo) => _scopeManager
        .OrElseThrow(() => new InvalidOperationException("Navigating to different tenant before initialization."))
        .OpenScope(tenantInfo);
}
