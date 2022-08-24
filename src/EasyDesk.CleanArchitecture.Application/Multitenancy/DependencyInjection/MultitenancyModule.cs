using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;

public class MultitenancyModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services, AppDescription app)
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
