using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Messages;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

public class MessageLoggingStep : IIncomingStep
{
    private readonly ILogger<MessageLoggingStep> _logger;

    public MessageLoggingStep(ILogger<MessageLoggingStep> logger)
    {
        _logger = logger;
    }

    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        var message = context.Load<Message>();
        var messageType = message.GetMessageType();
        _logger.LogInformation("Received asynchronous message with type {MessageType}.", messageType);

        await next();
    }
}
