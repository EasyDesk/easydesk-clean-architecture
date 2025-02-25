using Autofac;
using EasyDesk.CleanArchitecture.Application.Cancellation;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.Context.DependencyInjection;

public class ContextModule : AppModule
{
    protected override void ConfigureServices(AppDescription app, IServiceCollection services)
    {
        services.AddHttpContextAccessor();
    }

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.RegisterType<ContextDetector>()
            .SingleInstance();

        builder.RegisterType<ContextCancellationTokenProvider>()
            .As<ICancellationTokenProvider>()
            .SingleInstance();

        if (!app.IsMultitenant())
        {
            builder.RegisterType<PublicTenantProvider>()
                .As<ITenantProvider>()
                .IfNotRegistered(typeof(ITenantProvider))
                .SingleInstance();
        }
    }
}

public static class ContextModuleExtensions
{
    public static IAppBuilder AddContextDetector(this IAppBuilder builder)
    {
        return builder.AddModule(new ContextModule());
    }
}
