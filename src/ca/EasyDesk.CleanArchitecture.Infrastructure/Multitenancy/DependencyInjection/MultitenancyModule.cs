using Autofac;
using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Infrastructure.Multitenancy.DependencyInjection;

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
                .After(typeof(UnitOfWorkStep<,>))
                .After(typeof(AuthenticationStep<,>));
        });
    }

    protected override void ConfigureRegistry(AppDescription app, ServiceRegistry registry)
    {
        app.RequireModule<DataAccessModule>().Implementation.AddMultitenancy(registry, app);
    }

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.Register(_ => new TenantService())
            .As<IContextTenantInitializer>()
            .As<ITenantNavigator>()
            .As<IContextTenantNavigator>()
            .As<ITenantProvider>()
            .InstancePerUseCase();

        builder.RegisterInstance(Options.DefaultPolicy)
            .SingleInstance();
        builder.RegisterInstance(Options.HttpRequestTenantReader)
            .SingleInstance();
        builder.RegisterInstance(Options.AsyncMessageTenantReader)
            .SingleInstance();

        builder.RegisterType<ContextTenantDetector>()
            .As<IContextTenantDetector>()
            .InstancePerLifetimeScope();
    }
}

public static class MultitenancyModuleExtensions
{
    public static IAppBuilder AddMultitenancy(this IAppBuilder builder, Action<MultitenancyOptions>? configure = null)
    {
        return builder.AddModule(new MultitenancyModule(configure));
    }

    public static Option<MultitenancyOptions> GetMultitenancyOptions(this AppDescription app) => app.GetModule<MultitenancyModule>().Map(m => m.Options);

    public static bool IsMultitenant(this AppDescription app) => app.HasModule<MultitenancyModule>();
}
