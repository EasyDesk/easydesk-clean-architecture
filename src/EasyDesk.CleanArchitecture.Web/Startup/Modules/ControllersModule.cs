using System;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Tenants.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Json;
using EasyDesk.CleanArchitecture.Web.Filters;
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
        var mvcBuilder = services.AddControllers(options => DefaultMvcConfiguration(options, app));
        mvcBuilder.AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ApplyDefaultConfiguration();
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
        ////options.ModelBinderProviders.Insert(0, new TypedModelBinderProvider(new Dictionary<Type, Func<IModelBinder>>()
        ////{
        ////    { typeof(Date), ModelBindersFactory.ForDate },
        ////    { typeof(Timestamp), ModelBindersFactory.ForTimestamp },
        ////    { typeof(Duration), ModelBindersFactory.ForDuration },
        ////    { typeof(TimeOfDay), ModelBindersFactory.ForTimeOfDay }
        ////}));
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
