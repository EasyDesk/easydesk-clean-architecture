using Autofac;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.AutoScopingDispatchers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider.DependencyInjection;

public class ContextProviderModule : AppModule
{
    private readonly IHostEnvironment _environment;
    private Action<ContextProviderOptions>? _configure;

    public ContextProviderModule(IHostEnvironment environment, Action<ContextProviderOptions>? configure = null)
    {
        _environment = environment;
        _configure = configure;
    }

    public void ConfigureOptions(Action<ContextProviderOptions> configure)
    {
        _configure += configure;
    }

    public override void ConfigureServices(AppDescription app, IServiceCollection services, ContainerBuilder builder)
    {
        var options = new ContextProviderOptions();
        _configure?.Invoke(options);
        services.AddSingleton(options.AgentParser);

        services.AddHttpContextAccessor();
        if (_environment.IsDevelopment())
        {
            services.AddScoped<BasicContextProvider>();
            services.AddScoped(sp => new OverridableContextProvider(sp.GetRequiredService<BasicContextProvider>()));
            services.AddScoped<AutoScopingDispatcherFactory>();
            services.AddScoped<IContextProvider>(sp => sp.GetRequiredService<OverridableContextProvider>());
        }
        else
        {
            services.AddScoped<IContextProvider, BasicContextProvider>();
        }
        services.Decorate<IContextProvider, LazyContextProvider>();
        services.TryAddScoped<ITenantProvider, PublicTenantProvider>();
        services.TryAddSingleton<HttpRequestTenantReader>((_, _) => None);
        services.TryAddSingleton<AsyncMessageTenantReader>(_ => None);
    }
}

public static class ContextProviderModuleExtensions
{
    public static IAppBuilder AddContextProvider(this IAppBuilder builder, IHostEnvironment environment, Action<ContextProviderOptions>? configure = null)
    {
        return builder.AddModule(new ContextProviderModule(environment, configure));
    }

    public static IAppBuilder SetAgentParser(this IAppBuilder builder, Action<AgentParserBuilder> configure)
    {
        return builder.ConfigureModule<ContextProviderModule>(m => m.ConfigureOptions(x => x.SetAgentParser(configure)));
    }
}
