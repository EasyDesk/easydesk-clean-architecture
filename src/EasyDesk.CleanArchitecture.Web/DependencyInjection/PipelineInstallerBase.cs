using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.CleanArchitecture.Web.DependencyInjection
{
    public abstract class PipelineInstallerBase : IServiceInstaller
    {
        protected abstract Type ApplicationAssemblyMarker { get; }

        protected abstract Type InfrastructureAssemblyMarker { get; }

        protected abstract Type WebAssemblyMarker { get; }

        protected abstract bool UsesPublisher { get; }

        protected abstract bool UsesConsumer { get; }

        public void InstallServices(IServiceCollection services, IConfiguration config, IWebHostEnvironment environment)
        {
            services
                .AddControllersWithPipeline(environment)
                .AddRequestValidators(ApplicationAssemblyMarker)
                .AddMediatorWithPipeline(ApplicationAssemblyMarker, InfrastructureAssemblyMarker)
                .AddMappings(ApplicationAssemblyMarker, WebAssemblyMarker, InfrastructureAssemblyMarker);

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

            if (UsesConsumer || UsesPublisher)
            {
                var builder = services.AddEventManagement().AddAzureServiceBus(config);
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
