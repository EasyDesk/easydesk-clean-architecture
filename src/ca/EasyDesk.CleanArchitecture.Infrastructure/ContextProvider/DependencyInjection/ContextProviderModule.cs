using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider.DependencyInjection;

public class ContextProviderModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IContextProvider, BasicContextProvider>();
        services.AddScoped<IUserInfoProvider>(p => p.GetRequiredService<IContextProvider>());
        services.TryAddScoped<ITenantProvider, PublicTenantProvider>();
    }
}

public static class ContextProviderModuleExtensions
{
    public static AppBuilder AddContextProvider(this AppBuilder builder)
    {
        return builder.AddModule(new ContextProviderModule());
    }
}
