using System;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Tenants.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Json;
using EasyDesk.CleanArchitecture.Web.Filters;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Web.Startup.Modules;

public class ControllersModule : IAppModule
{
    private readonly IWebHostEnvironment _environment;
    private readonly Action<MvcOptions> _configureMvc;

    public ControllersModule(IWebHostEnvironment environment, Action<MvcOptions> configureMvc = null)
    {
        _environment = environment;
        _configureMvc = configureMvc;
    }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services
            .AddControllers(options => DefaultMvcConfiguration(options, app))
            .AddNewtonsoftJson(options =>
            {
                var dateTimeZoneProvider = app.GetModule<TimeManagementModule>()
                    .Map(m => m.DateTimeZoneProvider)
                    .OrElseNull();
                options.SerializerSettings.ApplyDefaultConfiguration(dateTimeZoneProvider);
            });
    }

    protected void DefaultMvcConfiguration(MvcOptions options, AppDescription app)
    {
        if (!_environment.IsDevelopment())
        {
            options.Filters.Add<UnhandledExceptionsFilter>();
        }
        if (app.IsMultitenant())
        {
            options.Filters.Add<TenantFilter>();
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
