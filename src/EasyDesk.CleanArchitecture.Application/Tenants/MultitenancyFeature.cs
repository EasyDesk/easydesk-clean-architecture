using EasyDesk.CleanArchitecture.Application.Features;
using EasyDesk.CleanArchitecture.Application.Tenants;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup.Features;

public class MultitenancyFeature : IAppFeature
{
    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddTenantManagement();
    }
}

public static class MultitenancyFeatureExtensions
{
    public static AppBuilder AddMultitenancy(this AppBuilder builder)
    {
        return builder.AddFeature(new MultitenancyFeature());
    }

    public static bool IsMultitenant(this AppDescription app) => app.HasFeature<MultitenancyFeature>();
}
