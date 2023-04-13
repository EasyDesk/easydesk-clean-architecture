using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider.DependencyInjection;

public class ContextProviderModule : AppModule
{
    private Action<ContextProviderOptions>? _configure;

    public ContextProviderModule(Action<ContextProviderOptions>? configure = null)
    {
        _configure = configure;
    }

    public void ConfigureOptions(Action<ContextProviderOptions> configure)
    {
        _configure += configure;
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        var options = new ContextProviderOptions();
        _configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddHttpContextAccessor();
        services.AddScoped<IContextProvider, BasicContextProvider>();
        services.AddScoped<IUserInfoProvider>(p => p.GetRequiredService<IContextProvider>());
        services.TryAddScoped<ITenantProvider, PublicTenantProvider>();
    }
}

public static class ContextProviderModuleExtensions
{
    public static AppBuilder AddContextProvider(this AppBuilder builder, Action<ContextProviderOptions>? configure = null)
    {
        return builder.AddModule(new ContextProviderModule(configure));
    }

    public static AppBuilder ConfigureContextProvider(this AppBuilder builder, Action<ContextProviderOptions> configure)
    {
        return builder.ConfigureModule<ContextProviderModule>(m => m.ConfigureOptions(configure));
    }
}
