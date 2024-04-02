using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Scopes;

namespace EasyDesk.CleanArchitecture.Testing.Unit.Commons;

public sealed class TestTenantManager
{
    private readonly ScopeManager<Option<TenantInfo>> _scopeManager;

    public TestTenantManager(Option<TenantInfo> defaultTenantInfo)
    {
        _scopeManager = new(defaultTenantInfo);
    }

    public Option<TenantInfo> CurrentTenantInfo => _scopeManager.Current;

    public Scope MoveTo(Option<TenantInfo> tenantInfo) => new(_scopeManager.OpenScope(tenantInfo));

    public Scope MoveToPublic() => MoveTo(Some(TenantInfo.Public));

    public Scope MoveToTenant(TenantId tenantId) => MoveTo(Some(TenantInfo.Tenant(tenantId)));

    public Scope Ignore() => MoveTo(None);

    public sealed class Scope : IDisposable
    {
        private readonly ScopeManager<Option<TenantInfo>>.Scope _innerScope;

        public Scope(ScopeManager<Option<TenantInfo>>.Scope innerScope)
        {
            _innerScope = innerScope;
        }

        public Option<TenantInfo> TenantInfo => _innerScope.Value;

        public void Dispose()
        {
            _innerScope.Dispose();
        }
    }
}
