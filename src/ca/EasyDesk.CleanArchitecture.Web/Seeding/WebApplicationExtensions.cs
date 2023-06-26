using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Web.Seeding;

public static class WebApplicationExtensions
{
    public static IDispatcher SetupSelfScopedDispatcher(this IServiceProvider services, ContextInfo? context = null, TenantId? tenantId = null) =>
        SetupSelfScopedDispatcher(services, services =>
        {
            var provider = services.GetRequiredService<OverridableContextProvider>();
            context.AsOption().IfPresent(provider.OverrideContextInfo);
            provider.OverrideTenantId(tenantId.AsOption().Map(x => x.Value));
        });

    private static IDispatcher SetupSelfScopedDispatcher(this IServiceProvider services, Action<IServiceProvider> setupScope) =>
        new AutoScopingDispatcher(services, x =>
        {
            setupScope(x);
            return Task.CompletedTask;
        });

    public static async Task SetupDevelopment(this WebApplication app, AsyncAction<IServiceProvider, ILogger> setup)
    {
        if (app.Environment.IsDevelopment())
        {
            await setup(app.Services, app.Logger);
        }
    }
}
