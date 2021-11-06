using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus
{
    internal class AzureServiceBusImplementation : IEventBusImplementation
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly string _connectionString;

        public AzureServiceBusImplementation(IConfiguration configuration, IWebHostEnvironment environment, string connectionString)
        {
            _configuration = configuration.GetSection("AzureServiceBusSettings");
            _environment = environment;
            _connectionString = connectionString;
        }

        private string TopicName => $"{_environment.EnvironmentName}/{_configuration.GetValue<string>("TopicName")}";

        private string SubscriptionName => _configuration.GetValue<string>("SubscriptionName");

        public void AddCommonServices(IServiceCollection services)
        {
            services.AddSingleton(_ => new ServiceBusClient(_connectionString));
            services.AddSingleton(_ => new ServiceBusAdministrationClient(_connectionString));
            services.AddSingleton<AzureServiceBusAdministrationFacade>();
            services.AddHostedService<AzureServiceBusSetup>();
        }

        public void AddPublisher(IServiceCollection services)
        {
            services.AddSingleton(AzureServiceBusSenderDescriptor.Topic(TopicName));
            services.AddSingleton<IEventBusPublisher, AzureServiceBusPublisher>();
        }

        public void AddConsumer(IServiceCollection services)
        {
            services.AddSingleton(AzureServiceBusReceiverDescriptor.Subscription(TopicName, SubscriptionName));
            services.AddSingleton<IEventBusConsumer, AzureServiceBusConsumer>();
        }
    }

    public static class AzureServiceBusExtensions
    {
        public static EventManagementBuilder AddAzureServiceBus(
            this EventBusImplementationBuilder builder,
            IConfiguration configuration,
            IWebHostEnvironment env,
            string connectionString)
        {
            return builder.AddEventBusImplementation(new AzureServiceBusImplementation(configuration, env, connectionString));
        }
    }
}
