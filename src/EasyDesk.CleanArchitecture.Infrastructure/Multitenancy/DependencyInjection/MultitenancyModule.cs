using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;

public class MultitenancyModule : AppModule
{
    private readonly Func<IServiceProvider, ITenantProvider> _tenantProviderFactory;

    public MultitenancyModule(Func<IServiceProvider, ITenantProvider> tenantProviderFactory)
    {
        _tenantProviderFactory = tenantProviderFactory;
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        if (_tenantProviderFactory is null)
        {
            services.AddScoped<ITenantProvider, DefaultTenantProvider>();
        }
        else
        {
            services.AddScoped(_tenantProviderFactory);
        }
    }
}

public static class MultitenancyModuleExtensions
{
    public static AppBuilder AddMultitenancy(this AppBuilder builder, Func<IServiceProvider, ITenantProvider> tenantProviderFactory = null)
    {
        return builder.AddModule(new MultitenancyModule(tenantProviderFactory));
    }

    public static bool IsMultitenant(this AppDescription app) => app.HasModule<MultitenancyModule>();
}
