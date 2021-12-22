using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Tenants
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTenantManagement(this IServiceCollection services)
        {
            return services
                .AddScoped<TenantService>()
                .AddScoped<ITenantInitializer>(p => p.GetRequiredService<TenantService>())
                .AddScoped<ITenantProvider>(p => p.GetRequiredService<TenantService>());
        }
    }
}
