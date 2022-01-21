using EasyDesk.CleanArchitecture.Application.Messaging.Receiver;
using EasyDesk.CleanArchitecture.Messaging.ServiceBus.Receiver;
using EasyDesk.CleanArchitecture.Messaging.ServiceBus.Sender;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Messaging.ServiceBus.Utils;

public class AzureServiceBusSetup : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AzureServiceBusAdministrationFacade _administrationFacade;

    public AzureServiceBusSetup(
        IServiceProvider serviceProvider,
        AzureServiceBusAdministrationFacade administrationFacade)
    {
        _serviceProvider = serviceProvider;
        _administrationFacade = administrationFacade;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var senderInfo = _serviceProvider.GetService<AzureServiceBusSenderDescriptor>();
        if (senderInfo is not null)
        {
            await senderInfo.Match(
                queue: q => _administrationFacade.CreateQueueIdempotent(q),
                topic: t => _administrationFacade.CreateTopicIdempotent(t));
        }

        var receiverInfo = _serviceProvider.GetService<AzureServiceBusReceiverDescriptor>();
        if (receiverInfo is not null)
        {
            await receiverInfo.Match(
                queue: q => _administrationFacade.CreateQueueIdempotent(q),
                subscription: async (t, s) =>
                {
                    var receiverDefinition = _serviceProvider.GetRequiredService<MessageReceiverDefinition>();
                    var filter = new MessageTypeFilter(receiverDefinition.SupportedTypes);
                    await _administrationFacade.CreateTopicIdempotent(t);
                    await _administrationFacade.CreateSubscriptionIdempotent(t, s, filter);
                });

            var receiver = _serviceProvider.GetRequiredService<IMessageReceiver>();
            await receiver.Start();
        }
    }
}
