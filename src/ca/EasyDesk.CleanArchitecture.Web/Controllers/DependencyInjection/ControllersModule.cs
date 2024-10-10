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
    private readonly IHostEnvironment _environment;

    public ControllersModuleOptions Options { get; } = new();

    public ControllersModule(IHostEnvironment environment, Action<ControllersModuleOptions>? configure = null)
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
        options.Filters.Add<FailedAuthenticationFilter>();
        options.Filters.Add<ExceptionLoggingFilter>();
        if (!_environment.IsDevelopment())
        {
            options.Filters.Add<UnhandledExceptionsFilter>();
        }
        options.Filters.Add<BadRequestFilter>();
        options.EnableEndpointRouting = false;
        options.ModelBinderProviders.Insert(0, new OptionBinderProvider());
        Options.ApplyMvcConfiguration(options);
    }
}

public static class ControllersModuleExtension
{
    public static IAppBuilder AddControllers(this IAppBuilder builder, IHostEnvironment environment, Action<ControllersModuleOptions>? configure = null)
    {
        return builder.AddModule(new ControllersModule(environment, configure));
    }
}
