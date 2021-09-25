using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus
{
    public class ErrorSafeEventBusMessageHandler : IEventBusMessageHandler
    {
        private readonly IEventBusMessageHandler _handler;
        private readonly ILogger<ErrorSafeEventBusMessageHandler> _logger;

        public ErrorSafeEventBusMessageHandler(IEventBusMessageHandler handler, ILogger<ErrorSafeEventBusMessageHandler> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        public async Task<EventBusMessageHandlerResult> Handle(EventBusMessage message)
        {
            try
            {
                return await _handler.Handle(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling an event of type {eventType}", message.EventType);
                return EventBusMessageHandlerResult.GenericFailure;
            }
        }
    }
}
