using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.Tools.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus
{
    public class AzureServiceBus : IEventBusImplementation
    {
        private readonly IConfiguration _configuration;
        private readonly Option<string> _prefix;
        private readonly string _connectionString;

        public AzureServiceBus(IConfiguration configuration)
            : this(configuration, None)
        {
        }

        public AzureServiceBus(IConfiguration configuration, string prefix)
            : this(configuration, Some(prefix))
        {
        }

        private AzureServiceBus(IConfiguration configuration, Option<string> prefix)
        {
            _configuration = configuration;
            _prefix = prefix;
            _connectionString = configuration.RequireConnectionString("AzureServiceBus");
        }

        private string TopicNameWithoutPrefix => GetValueInsideConfigSection("TopicName");

        private string TopicName => _prefix.Match(
            some: p => $"{p}/{TopicNameWithoutPrefix}",
            none: () => TopicNameWithoutPrefix);

        private string SubscriptionName => GetValueInsideConfigSection("SubscriptionName");

        private string GetValueInsideConfigSection(string key) =>
            _configuration.RequireValue<string>($"AzureServiceBusSettings:{key}");

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
