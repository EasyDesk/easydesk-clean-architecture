using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.Receiver;

namespace EasyDesk.CleanArchitecture.Application.ExternalEvents;

public class ExternalEventMessageHandler : IMessageHandler
{
    private readonly IExternalEventHandler _externalEventHandler;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IDictionary<string, Type> _supportedTypes;

    public ExternalEventMessageHandler(
        IExternalEventHandler externalEventHandler,
        IJsonSerializer jsonSerializer,
        IEnumerable<Type> supportedTypes)
    {
        _externalEventHandler = externalEventHandler;
        _jsonSerializer = jsonSerializer;
        _supportedTypes = supportedTypes.ToDictionary(t => t.GetEventTypeName());
    }

    public async Task<MessageHandlerResult> Handle(Message message)
    {
        if (!_supportedTypes.TryGetValue(message.Type, out var eventType))
        {
            return MessageHandlerResult.NotSupported;
        }
        return await HandleMessageWithType(eventType, message.Content);
    }

    private async Task<MessageHandlerResult> HandleMessageWithType(Type eventType, string message)
    {
        var externalEvent = _jsonSerializer.Deserialize(message, eventType) as ExternalEvent;
        var handlerResponse = await _externalEventHandler.Handle(externalEvent);
        return handlerResponse.Match(
            success: _ => MessageHandlerResult.Handled,
            failure: _ => MessageHandlerResult.GenericFailure);
    }
}
