using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;
using EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus;
using EasyDesk.CleanArchitecture.Web.Converters;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.CleanArchitecture.Web.Filters;
using EasyDesk.CleanArchitecture.Web.ModelBinders;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Web.DependencyInjection
{
    public abstract class PipelineInstallerBase : IServiceInstaller
    {
        private const string DefaultServiceBusConnectionStringName = "AzureServiceBus";

        protected abstract Type ApplicationAssemblyMarker { get; }

        protected abstract Type InfrastructureAssemblyMarker { get; }

        protected abstract Type WebAssemblyMarker { get; }

        protected abstract bool UsesPublisher { get; }

        protected abstract bool UsesConsumer { get; }

        protected virtual string ServiceBusConnectionStringName => DefaultServiceBusConnectionStringName;

        public void InstallServices(IServiceCollection services, IConfiguration config, IWebHostEnvironment environment)
        {
            AddControllersWithPipeline(services, config, environment);
            AddMediatr(services);
            AddRequestValidators(services);
            AddMappings(services);
            AddApiVersioning(services);
            AddEventManagement(services, config, environment);
        }

        private void AddControllersWithPipeline(IServiceCollection services, IConfiguration config, IWebHostEnvironment environment)
        {
            services
                .AddControllers(options => ConfigureMvc(options, config, environment))
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

            services.AddSingleton(provider => provider
                .GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>()
                .Value
                .SerializerSettings);
        }

        protected virtual void ConfigureMvc(MvcOptions options, IConfiguration config, IWebHostEnvironment environment)
        {
            options.EnableEndpointRouting = false;
            if (!environment.IsDevelopment())
            {
                options.Filters.Add<UnhandledExceptionsFilter>();
            }
            options.Filters.Add<TenantFilter>();
            options.ModelBinderProviders.Insert(0, new TypedModelBinderProvider(new Dictionary<Type, Func<IModelBinder>>()
            {
                { typeof(Date), ModelBindersFactory.ForDate },
                { typeof(Timestamp), ModelBindersFactory.ForTimestamp },
                { typeof(Duration), ModelBindersFactory.ForDuration },
                { typeof(TimeOfDay), ModelBindersFactory.ForTimeOfDay }
            }));
        }

        private void AddMediatr(IServiceCollection services)
        {
            services
                .AddMediatR(ApplicationAssemblyMarker, InfrastructureAssemblyMarker)
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviorWrapper<,>));
        }

        private void AddRequestValidators(IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining(ApplicationAssemblyMarker);
        }

        private void AddMappings(IServiceCollection services)
        {
            services.AddAutoMapper(config =>
            {
                config.AddProfile(new DefaultMappingProfile(
                    typeof(SupportedVersionDto),
                    ApplicationAssemblyMarker,
                    WebAssemblyMarker,
                    InfrastructureAssemblyMarker));
            });
        }

        private void AddApiVersioning(IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;

                options.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("version"),
                    new HeaderApiVersionReader("Api-Version"));

                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = VersioningUtils.GetSupportedVersions(WebAssemblyMarker)
                    .MaxOption()
                    .OrElseGet(() => VersioningUtils.DefaultVersion);

                options.Conventions.Add(new NamespaceConvention());
            });
        }

        private void AddEventManagement(IServiceCollection services, IConfiguration config, IWebHostEnvironment environment)
        {
            if (UsesConsumer || UsesPublisher)
            {
                var builder = services
                    .AddEventManagement()
                    .AddAzureServiceBus(config, environment, config.GetConnectionString(ServiceBusConnectionStringName));
                if (UsesPublisher)
                {
                    builder.AddOutboxPublisher();
                }
                if (UsesConsumer)
                {
                    builder.AddIdempotentConsumer(ApplicationAssemblyMarker);
                }
            }
        }

        private class NamespaceConvention : IControllerConvention
        {
            public bool Apply(IControllerConventionBuilder controller, ControllerModel controllerModel)
            {
                VersioningUtils.GetControllerVersion(controllerModel.ControllerType)
                    .Match(
                        some: v => controller.HasApiVersion(v),
                        none: () => controller.IsApiVersionNeutral());
                return true;
            }
        }
    }
}
