using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.Receiver;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Receiver;

public class ErrorSafeMessageHandler : IMessageHandler
{
    private readonly IMessageHandler _handler;
    private readonly ILogger<ErrorSafeMessageHandler> _logger;

    public ErrorSafeMessageHandler(IMessageHandler handler, ILogger<ErrorSafeMessageHandler> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task<MessageHandlerResult> Handle(Message message)
    {
        try
        {
            return await _handler.Handle(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while handling a message of type {messageType}", message.Content.GetType().Name);
            return MessageHandlerResult.GenericFailure;
        }
    }
}
