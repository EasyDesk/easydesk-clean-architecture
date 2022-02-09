using Azure.Core;
using Rebus.AzureServiceBus.NameFormat;
using Rebus.Config;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Messaging.ServiceBus;

public static class TransportExtensions
{
    public static AzureServiceBusTransportSettings UseAzureServiceBusWithinEnvironment(
        this StandardConfigurer<ITransport> configurer,
        string connectionString,
        string inputQueueAddress,
        string environmentName,
        TokenCredential tokenCredential = null)
    {
        var settings = configurer.UseAzureServiceBus(connectionString, inputQueueAddress, tokenCredential);
        SetEnvironmentPrefix(configurer, environmentName);
        return settings;
    }

    public static AzureServiceBusTransportClientSettings UseAzureServiceBusWithinEnvironmentAsOneWayClient(
        this StandardConfigurer<ITransport> configurer,
        string connectionString,
        string environmentName,
        TokenCredential tokenCredential = null)
    {
        var settings = configurer.UseAzureServiceBusAsOneWayClient(connectionString, tokenCredential);
        SetEnvironmentPrefix(configurer, environmentName);
        return settings;
    }

    private static void SetEnvironmentPrefix(StandardConfigurer<ITransport> configurer, string environmentName)
    {
        configurer.OtherService<INameFormatter>().Decorate(c =>
        {
            return new PrefixNameFormatter($"{environmentName}/", c.Get<INameFormatter>());
        });
    }
}
