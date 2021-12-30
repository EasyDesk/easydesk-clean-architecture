using EasyDesk.CleanArchitecture.Web.Converters;
using EasyDesk.CleanArchitecture.Web.Filters;
using EasyDesk.CleanArchitecture.Web.ModelBinders;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Web.Startup
{
    public partial class BaseStartup
    {
        private void AddControllersWithPipeline(IServiceCollection services)
        {
            services
                .AddControllers(DefaultMvcConfiguration)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DateParseHandling = DateParseHandling.None;

                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.Converters.Add(JsonConverters.FromStringParser(Date.Parse));
                    options.SerializerSettings.Converters.Add(JsonConverters.FromStringParser(Timestamp.Parse));
                    options.SerializerSettings.Converters.Add(JsonConverters.FromStringParser(TimeOfDay.Parse));
                    options.SerializerSettings.Converters.Add(JsonConverters.FromStringParser(Duration.Parse));
                    options.SerializerSettings.Converters.Add(JsonConverters.FromStringParser(LocalDateTime.Parse));
                });

            // TODO: refactor this.
            services.AddSingleton(provider => provider
                .GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>()
                .Value
                .SerializerSettings);
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
}
