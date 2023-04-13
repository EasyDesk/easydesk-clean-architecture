using EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;

public sealed class ControllersModuleOptions
{
    public const int DefaultDefaultPageSize = 100;
    public const int DefaultMaxPageSize = 100;
    private Action<MvcOptions>? _configureMvc;

    public int DefaultPageSize { get; set; } = DefaultDefaultPageSize;

    public int MaxPageSize { get; set; } = DefaultMaxPageSize;

    public void ConfigureMvc(Action<MvcOptions> configureMvc) => _configureMvc += configureMvc;

    internal void ApplyMvcConfiguration(MvcOptions options) => _configureMvc?.Invoke(options);
}

public class ControllersModule : AppModule
{
    private readonly IWebHostEnvironment _environment;

    public ControllersModuleOptions Options { get; } = new();

    public ControllersModule(IWebHostEnvironment environment, Action<ControllersModuleOptions>? configure = null)
    {
        _environment = environment;
        configure?.Invoke(Options);
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services
            .AddControllers(DefaultMvcConfiguration)
            .AddApplicationPart(typeof(CleanArchitectureController).Assembly)
            .AddNewtonsoftJson(options =>
            {
                app.RequireModule<JsonModule>().ApplyJsonConfiguration(options.SerializerSettings, app);
            });

        services.AddSingleton(new PaginationService(Options.DefaultPageSize, Options.MaxPageSize));
    }

    protected void DefaultMvcConfiguration(MvcOptions options)
    {
        if (_environment.IsProduction())
        {
            options.Filters.Add<UnhandledExceptionsFilter>();
        }
        options.Filters.Add<BadRequestFilter>();
        options.EnableEndpointRouting = false;
        Options.ApplyMvcConfiguration(options);
    }
}

public static class ControllersModuleExtension
{
    public static AppBuilder AddControllers(this AppBuilder builder, IWebHostEnvironment environment, Action<ControllersModuleOptions>? configure = null)
    {
        return builder.AddModule(new ControllersModule(environment, configure));
    }
}
