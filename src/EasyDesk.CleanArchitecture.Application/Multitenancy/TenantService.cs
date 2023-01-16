using EasyDesk.Tools.Collections;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

internal class TenantService : ITenantNavigator, IContextTenantInitializer
{
    private readonly Stack<TenantScope> _scopes = new();

    public void Initialize(TenantInfo tenantInfo)
    {
        if (!_scopes.IsEmpty())
        {
            throw new InvalidOperationException("Trying to initialize tenant after it was already initialized");
        }
#pragma warning disable CA2000 // Dispose objects before losing scope
        _scopes.Push(new TenantScope(tenantInfo, _ => throw new InvalidOperationException("Can't dispose the default context tenant scope.")));
#pragma warning restore CA2000 // Dispose objects before losing scope
    }

    private TenantScope Open(TenantInfo tenantInfo)
    {
        if (_scopes.IsEmpty())
        {
            throw new InvalidOperationException("Opening scope before tenant initialization");
        }
        var scope = new TenantScope(tenantInfo, scope =>
        {
            if (!_scopes.TryPeek(out var topScope))
            {
                throw new InvalidOperationException("Closing scope before tenant initialization");
            }
            if (!ReferenceEquals(scope, topScope))
            {
                throw new InvalidOperationException("The current scope doesn't match the exiting scope. A dispose call is probably missing somewhere.");
            }
            _scopes.Pop();
        });
        _scopes.Push(scope);
        return scope;
    }

    public ITenantScope MoveToTenant(TenantId id) => Open(TenantInfo.Tenant(id));

    public ITenantScope MoveToPublic() => Open(TenantInfo.Public);

#pragma warning disable CA2000 // Dispose objects before losing scope
    public TenantInfo TenantInfo =>
        _scopes.TryPeek(out var scope)
            ? scope.TenantInfo
            : throw new InvalidOperationException("Accessing tenant before initialization");
#pragma warning restore CA2000 // Dispose objects before losing scope

    private sealed class TenantScope : ITenantScope, ITenantProvider
    {
        private readonly Action<TenantScope> _onDispose;

        public TenantScope(TenantInfo tenantInfo, Action<TenantScope> onDispose)
        {
            TenantInfo = tenantInfo;
            _onDispose = onDispose;
        }

        public TenantInfo TenantInfo { get; }

        public void Dispose() => _onDispose(this);
    }
}
