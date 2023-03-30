using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;

public class MultitenancyModule : AppModule
{
    public MultitenancyModule(Action<MultitenancyOptions>? configureOptions = null)
    {
        configureOptions?.Invoke(Options);
    }

    public MultitenancyOptions Options { get; } = new();

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline =>
        {
            pipeline
                .AddStep(typeof(MultitenancyManagementStep<,>))
                .After(typeof(UnitOfWorkStep<,>));
        });
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped(Options.TenantReaderImplementation);

        services.AddScoped<TenantService>();
        services.AddScoped<IContextTenantInitializer>(p => p.GetRequiredService<TenantService>());
        services.AddScoped<ITenantNavigator>(p => p.GetRequiredService<TenantService>());
        services.AddScoped<ITenantProvider>(p => p.GetRequiredService<ITenantNavigator>());

        app.RequireModule<DataAccessModule>().Implementation.AddMultitenancy(services, app);

        services.AddSingleton(Options.DefaultPolicy);
    }
}

public static class MultitenancyModuleExtensions
{
    public static AppBuilder AddMultitenancy(this AppBuilder builder, Action<MultitenancyOptions>? configure = null)
    {
        return builder.AddModule(new MultitenancyModule(configure));
    }

    public static bool IsMultitenant(this AppDescription app) => app.HasModule<MultitenancyModule>();
}
