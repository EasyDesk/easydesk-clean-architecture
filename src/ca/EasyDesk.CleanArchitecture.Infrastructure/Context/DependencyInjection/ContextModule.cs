using Autofac;
using EasyDesk.CleanArchitecture.Application.Cancellation;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
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
    }
}

public static class ContextModuleExtensions
{
    public static IAppBuilder AddContextDetector(this IAppBuilder builder)
    {
        return builder.AddModule(new ContextModule());
    }
}
