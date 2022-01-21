using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker;
using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.Options;

namespace EasyDesk.CleanArchitecture.Application.ExternalEvents;

public class ExternalEventPublisher : IExternalEventPublisher
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly ITimestampProvider _timestampProvider;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ITenantProvider _tenantProvider;

    public ExternalEventPublisher(
        IMessagePublisher messagePublisher,
        ITimestampProvider timestampProvider,
        IJsonSerializer jsonSerializer,
        ITenantProvider tenantProvider)
    {
        _messagePublisher = messagePublisher;
        _timestampProvider = timestampProvider;
        _jsonSerializer = jsonSerializer;
        _tenantProvider = tenantProvider;
    }

    public async Task Publish(IEnumerable<ExternalEvent> events)
    {
        var messages = events.Select(e => ToMessage(e)).ToList();
        await _messagePublisher.Publish(messages);
    }

    private Message ToMessage(ExternalEvent externalEvent)
    {
        return new Message(
            Id: Guid.NewGuid(),
            Timestamp: _timestampProvider.Now,
            Type: externalEvent.GetType().GetEventTypeName(),
            TenantId: _tenantProvider.TenantId,
            Content: _jsonSerializer.Serialize(externalEvent));
    }
}
