using EasyDesk.Commons.Results;
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
        var messageId = message.GetMessageId();

        try
        {
            _logger.LogInformation("Message [Type={MessageType}, ID={MessageId}]: Received", messageType, messageId);
            await next();
            _logger.LogInformation("Message [Type={MessageType}, ID={MessageId}]: Successful", messageType, messageId);
        }
        catch (ResultFailedException e)
        {
            _logger.LogError("Message [Type={MessageType}, ID={MessageId}]: Error - Type={ErrorType}", messageType, messageId, e.Error.GetType().Name);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("Message [Type={MessageType}, ID={MessageId}]: Exception - Type={ExceptionType}", messageType, messageId, e.GetType().Name);
            throw;
        }
    }
}
