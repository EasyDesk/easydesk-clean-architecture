using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Tenants.DependencyInjection;

public class MultitenancyModule : IAppModule
{
    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services
            .AddScoped<TenantService>()
            .AddScoped<ITenantInitializer>(p => p.GetRequiredService<TenantService>())
            .AddScoped<ITenantProvider>(p => p.GetRequiredService<TenantService>());
    }
}

public static class MultitenancyModuleExtensions
{
    public static AppBuilder AddMultitenancy(this AppBuilder builder)
    {
        return builder.AddModule(new MultitenancyModule());
    }

    public static bool IsMultitenant(this AppDescription app) => app.HasModule<MultitenancyModule>();
}
