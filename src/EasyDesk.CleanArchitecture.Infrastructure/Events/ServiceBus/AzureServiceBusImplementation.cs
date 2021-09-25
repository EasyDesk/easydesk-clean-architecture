using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus
{
    internal class AzureServiceBusImplementation : IEventBusImplementation
    {
        private readonly IConfiguration _configuration;

        public AzureServiceBusImplementation(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void AddCommonServices(IServiceCollection services)
        {
            var settings = services.AddConfigAsSingleton<AzureServiceBusSettings>(_configuration);
            services.AddSingleton(_ => new ServiceBusClient(settings.ConnectionString));
            services.AddSingleton(_ => new ServiceBusAdministrationClient(settings.ConnectionString));
            services.AddSingleton<AzureServiceBusAdministrationFacade>();
            services.AddSingleton<IEventBusSetup, AzureServiceBusSetup>();
        }

        public void AddPublisher(IServiceCollection services)
        {
            services.AddSingleton<IEventBusPublisher, AzureServiceBusPublisher>();
        }

        public void AddConsumer(IServiceCollection services)
        {
            services.AddSingleton<IEventBusConsumer, AzureServiceBusConsumer>();
        }
    }

    public static class AzureServiceBusExtensions
    {
        public static EventManagementBuilder AddAzureServiceBus(this EventBusImplementationBuilder builder, IConfiguration configuration)
        {
            return builder.AddEventBusImplementation(new AzureServiceBusImplementation(configuration));
        }
    }
}
