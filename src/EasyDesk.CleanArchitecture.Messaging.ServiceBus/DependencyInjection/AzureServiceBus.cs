using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging.Receiver;
using EasyDesk.CleanArchitecture.Application.Messaging.Sender;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.CleanArchitecture.Messaging.ServiceBus.Receiver;
using EasyDesk.CleanArchitecture.Messaging.ServiceBus.Sender;
using EasyDesk.CleanArchitecture.Messaging.ServiceBus.Utils;
using EasyDesk.Tools.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Messaging.ServiceBus.DependencyInjection;

public class AzureServiceBus : IMessageBrokerImplementation
{
    private readonly IConfiguration _configuration;
    private readonly Option<string> _prefix;

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
    }

    private string ConnectionString => _configuration.RequireConnectionString("AzureServiceBus");

    private string TopicNameWithoutPrefix => GetValueInsideConfigSection("TopicName");

    private string TopicName => _prefix.Match(
        some: p => $"{p}/{TopicNameWithoutPrefix}",
        none: () => TopicNameWithoutPrefix);

    private string SubscriptionName => GetValueInsideConfigSection("SubscriptionName");

    private string GetValueInsideConfigSection(string key) =>
        _configuration.RequireValue<string>($"AzureServiceBusSettings:{key}");

    public void AddCommonServices(IServiceCollection services)
    {
        services.AddSingleton(_ => new ServiceBusClient(ConnectionString));
        services.AddSingleton(_ => new ServiceBusAdministrationClient(ConnectionString));
        services.AddSingleton<AzureServiceBusAdministrationFacade>();
        services.AddHostedService<AzureServiceBusSetup>();
    }

    public void AddMessageSender(IServiceCollection services)
    {
        services.AddSingleton(AzureServiceBusSenderDescriptor.Topic(TopicName));
        services.AddSingleton<IMessageSender, AzureServiceBusSender>();
    }

    public void AddMessageReceiver(IServiceCollection services)
    {
        services.AddSingleton(AzureServiceBusReceiverDescriptor.Subscription(TopicName, SubscriptionName));
        services.AddSingleton<IMessageReceiver, AzureServiceBusReceiver>();
    }
}
