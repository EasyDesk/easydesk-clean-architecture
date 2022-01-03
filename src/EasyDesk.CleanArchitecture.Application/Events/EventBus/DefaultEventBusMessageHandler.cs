using EasyDesk.CleanArchitecture.Application.Events.ExternalEvents;
using EasyDesk.CleanArchitecture.Application.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus;

public class DefaultEventBusMessageHandler : IEventBusMessageHandler
{
    private readonly IExternalEventHandler _externalEventHandler;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IDictionary<string, Type> _supportedTypes;

    public DefaultEventBusMessageHandler(
        IExternalEventHandler externalEventHandler,
        IJsonSerializer jsonSerializer,
        IEnumerable<Type> supportedTypes)
    {
        _externalEventHandler = externalEventHandler;
        _jsonSerializer = jsonSerializer;
        _supportedTypes = supportedTypes.ToDictionary(t => t.GetEventTypeName());
    }

    public async Task<EventBusMessageHandlerResult> Handle(EventBusMessage message)
    {
        if (!_supportedTypes.TryGetValue(message.EventType, out var eventType))
        {
            return EventBusMessageHandlerResult.NotSupported;
        }
        return await HandleMessageWithType(eventType, message.Content);
    }

    private async Task<EventBusMessageHandlerResult> HandleMessageWithType(Type eventType, string message)
    {
        var externalEvent = _jsonSerializer.Deserialize(message, eventType) as ExternalEvent;
        var handlerResponse = await _externalEventHandler.Handle(externalEvent);
        return handlerResponse.Match(
            success: _ => EventBusMessageHandlerResult.Handled,
            failure: _ => EventBusMessageHandlerResult.GenericFailure);
    }
}
