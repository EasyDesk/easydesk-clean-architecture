using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Inbox;

public class InboxStep : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        var messageId = context.Load<TransportMessage>().GetMessageId();
        var inbox = context
            .Load<ITransactionContext>()
            .GetServiceProvider()
            .GetRequiredService<IInbox>();

        if (await inbox.HasBeenProcessed(messageId))
        {
            return;
        }

        await next();

        await inbox.MarkAsProcessed(messageId);
    }
}
