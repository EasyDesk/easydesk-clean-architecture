using EasyDesk.CleanArchitecture.Application.Features;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Tenants;

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
