using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.Multitenancy.DependencyInjection;

public class MultitenancyModule : AppModule
{
    public MultitenancyModule(Action<MultitenancyOptions> configureOptions = null)
    {
        configureOptions?.Invoke(Options);
    }

    public MultitenancyOptions Options { get; } = new();

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.RequireModule<DispatchingModule>().Pipeline
            .AddStep(typeof(TenantRequirementStep<,>));
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped<ContextTenantReader>();

        services.AddScoped<ITenantNavigator, TenantNavigator>();
        services.AddScoped<ITenantProvider>(p => p.GetRequiredService<ITenantNavigator>());

        app.RequireModule<DataAccessModule>().Implementation.AddMultitenancy(services, app);

        services.AddSingleton(Options);
    }
}

public static class MultitenancyModuleExtensions
{
    public static AppBuilder AddMultitenancy(this AppBuilder builder, Action<MultitenancyOptions> configure = null)
    {
        return builder.AddModule(new MultitenancyModule(configure));
    }

    public static AppBuilder ConfigureMultitenancy(this AppBuilder builder, Action<MultitenancyOptions> configure)
    {
        return builder.ConfigureModule<MultitenancyModule>(m => configure(m.Options));
    }

    public static bool IsMultitenant(this AppDescription app) => app.HasModule<MultitenancyModule>();
}
