using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.AutoScopingDispatchers;

public class AutoScopingDispatcherFactory
{
    private readonly IServiceProvider _services;

    public AutoScopingDispatcherFactory(IServiceProvider services)
    {
        _services = services;
    }

    public IDispatcher CreateAutoScopingDispatcher(ContextInfo? context = null, TenantId? tenantId = null) =>
        CreateAutoScopingDispatcher(services =>
        {
            var provider = services.GetRequiredService<OverridableContextProvider>();
            context.AsOption().IfPresent(provider.OverrideContextInfo);
            provider.OverrideTenantId(tenantId.AsOption().Map(x => x.Value));
        });

    private IDispatcher CreateAutoScopingDispatcher(Action<IServiceProvider> setupScope) =>
        new AutoScopingDispatcher(_services, x =>
        {
            setupScope(x);
            return Task.CompletedTask;
        });
}
