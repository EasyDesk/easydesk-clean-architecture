using EasyDesk.CleanArchitecture.Infrastructure.Json;
using EasyDesk.CleanArchitecture.Web.Filters;
using EasyDesk.CleanArchitecture.Web.ModelBinders;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Web.Startup;

public partial class BaseStartup
{
    private void AddControllersWithPipeline(IServiceCollection services)
    {
        services
            .AddControllers(DefaultMvcConfiguration)
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ApplyDefaultConfiguration();
                ConfigureJsonSerializerSettings(options.SerializerSettings);
            });
    }

    protected void DefaultMvcConfiguration(MvcOptions options)
    {
        if (!Environment.IsDevelopment())
        {
            options.Filters.Add<UnhandledExceptionsFilter>();
        }
        if (IsMultitenant)
        {
            options.Filters.Add<TenantFilter>();
        }
        options.EnableEndpointRouting = false;
        options.ModelBinderProviders.Insert(0, new TypedModelBinderProvider(new Dictionary<Type, Func<IModelBinder>>()
            {
                { typeof(Date), ModelBindersFactory.ForDate },
                { typeof(Timestamp), ModelBindersFactory.ForTimestamp },
                { typeof(Duration), ModelBindersFactory.ForDuration },
                { typeof(TimeOfDay), ModelBindersFactory.ForTimeOfDay }
            }));
        ConfigureMvc(options);
    }

    protected virtual void ConfigureMvc(MvcOptions options)
    {
    }
}
