using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;

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
        if (Options.TenantProviderFactory is null)
        {
            services.AddScoped<ITenantProvider, DefaultTenantProvider>();
        }
        else
        {
            services.AddScoped(Options.TenantProviderFactory);
        }

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
