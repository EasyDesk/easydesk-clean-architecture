using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus
{
    public class AzureServiceBusImplementation : IEventBusImplementation
    {
        private const string SettingsSectionName = "AzureServiceBusSettings";

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly string _connectionString;

        public AzureServiceBusImplementation(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            _connectionString = configuration.GetRequiredConnectionString("AzureServiceBus");
        }

        private string TopicName => $"{_environment.EnvironmentName}/{_configuration.GetRequiredValue<string>($"{SettingsSectionName}:TopicName")}";

        private string SubscriptionName => _configuration.GetRequiredValue<string>($"{SettingsSectionName}:SubscriptionName");

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
}
