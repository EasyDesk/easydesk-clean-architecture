using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Scopes;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;

public record DefaultTenantInfo(Option<TenantInfo> TenantInfo);

public sealed class TestTenantManager : IHttpRequestConfigurator
{
    private readonly ScopeManager<Option<TenantInfo>> _scopeManager;

    public TestTenantManager(DefaultTenantInfo defaultTenantInfo)
    {
        _scopeManager = new(defaultTenantInfo.TenantInfo);
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

    public void ConfigureHttpRequest(HttpRequestBuilder request)
    {
        CurrentTenantInfo.FlatMap(x => x.Id).Match(
            some: request.Tenant,
            none: request.NoTenant);
    }
}
