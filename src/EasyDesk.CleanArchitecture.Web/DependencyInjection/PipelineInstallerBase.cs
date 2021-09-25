using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus;
using Microsoft.AspNetCore.Hosting;
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
    }
}
