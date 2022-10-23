using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.Messages;
using Microsoft.Extensions.Logging;

namespace EasyDesk.SampleApp.Application.ExternalEventHandlers;

public class LogAllMessages : IMessageHandler<IIncomingMessage>
{
    private readonly ILogger<LogAllMessages> _logger;

    public LogAllMessages(ILogger<LogAllMessages> logger)
    {
        _logger = logger;
    }

    public Task<Result<Nothing>> Handle(IIncomingMessage message)
    {
        _logger.LogInformation("Message received: {message}", message);
        return Task.FromResult(Ok);
    }
}
