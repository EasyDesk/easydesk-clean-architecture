using EasyDesk.Commons.Scopes;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Dispatching;

internal class DispatcherScopeManager
{
    private static readonly AsyncLocal<DispatcherScopeManager> _asyncLocalInstance = new();
    private readonly ScopeManager<IServiceProvider> _scopeManager;

    internal static DispatcherScopeManager? Current
    {
        get => _asyncLocalInstance.Value;
        set => _asyncLocalInstance.Value = value!;
    }

    public DispatcherScopeManager(IServiceProvider rootServiceProvider)
    {
        _scopeManager = new(rootServiceProvider);
        RootServiceProvider = rootServiceProvider;
    }

    public IServiceProvider RootServiceProvider { get; }

    public int Depth => _scopeManager.Depth;

    public Scope OpenNewScope()
    {
        var serviceScope = _scopeManager.Current.CreateAsyncScope();
        var scopeManagerScope = _scopeManager.OpenScope(serviceScope.ServiceProvider);
        return new Scope(serviceScope, scopeManagerScope);
    }

    public sealed class Scope : IAsyncDisposable
    {
        private readonly AsyncServiceScope _serviceScope;
        private readonly ScopeManager<IServiceProvider>.Scope _scopeManagerScope;

        public Scope(AsyncServiceScope serviceScope, ScopeManager<IServiceProvider>.Scope scopeManagerScope)
        {
            _serviceScope = serviceScope;
            _scopeManagerScope = scopeManagerScope;
        }

        public IServiceProvider ServiceProvider => _serviceScope.ServiceProvider;

        public async ValueTask DisposeAsync()
        {
            await _serviceScope.DisposeAsync();
            _scopeManagerScope.Dispose();
        }
    }
}
