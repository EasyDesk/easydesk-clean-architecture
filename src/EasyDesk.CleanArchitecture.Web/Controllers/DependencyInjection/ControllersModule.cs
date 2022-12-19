using EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;

public class ControllersModule : AppModule
{
    private readonly IWebHostEnvironment _environment;
    private readonly Action<MvcOptions> _configureMvc;

    public ControllersModule(IWebHostEnvironment environment, Action<MvcOptions> configureMvc = null)
    {
        _environment = environment;
        _configureMvc = configureMvc;
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
    }

    protected void DefaultMvcConfiguration(MvcOptions options)
    {
        if (_environment.IsProduction())
        {
            options.Filters.Add<UnhandledExceptionsFilter>();
        }
        options.EnableEndpointRouting = false;
        _configureMvc?.Invoke(options);
    }
}

public static class ControllersModuleExtension
{
    public static AppBuilder AddControllers(this AppBuilder builder, IWebHostEnvironment environment, Action<MvcOptions> configureMvc = null)
    {
        return builder.AddModule(new ControllersModule(environment, configureMvc));
    }
}
