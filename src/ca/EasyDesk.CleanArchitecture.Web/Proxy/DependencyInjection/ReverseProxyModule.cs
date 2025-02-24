using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Proxy.DependencyInjection;

public class ReverseProxyModule : AppModule
{
    private readonly ReverseProxyModuleOptions _options;

    public ReverseProxyModule(Action<ReverseProxyModuleOptions>? configure)
    {
        var options = new ReverseProxyModuleOptions();
        configure?.Invoke(options);
        _options = options;
    }

    protected override void ConfigureServices(AppDescription app, IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
            _options.ConfigureMiddleware?.Invoke(options);
        });
    }
}

public static class ReverseProxyModuleExtensions
{
    public static IAppBuilder AddReverseProxy(this IAppBuilder builder, Action<ReverseProxyModuleOptions>? configure = null)
    {
        return builder.AddModule(new ReverseProxyModule(configure));
    }

    public static bool HasReverseProxy(this AppDescription app) => app.HasModule<ReverseProxyModule>();

    public static void UseReverseProxyModule(this WebApplication app)
    {
        app.UseForwardedHeaders();
    }
}

public sealed class ReverseProxyModuleOptions
{
    public Action<ForwardedHeadersOptions>? ConfigureMiddleware { get; set; }
}
