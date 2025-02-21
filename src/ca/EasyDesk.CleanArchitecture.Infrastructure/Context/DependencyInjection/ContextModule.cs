using Autofac;
using EasyDesk.CleanArchitecture.Application.Cancellation;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyDesk.CleanArchitecture.Infrastructure.Context.DependencyInjection;

public class ContextModule : AppModule
{
    public override void ConfigureServices(AppDescription app, IServiceCollection services, ContainerBuilder builder)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<ContextDetector>();
        services.AddSingleton<ICancellationTokenProvider, ContextCancellationTokenProvider>();
        services.TryAddScoped<ITenantProvider, PublicTenantProvider>();
    }
}

public static class ContextProviderModuleExtensions
{
    public static IAppBuilder AddContextDetector(this IAppBuilder builder)
    {
        return builder.AddModule(new ContextModule());
    }
}
