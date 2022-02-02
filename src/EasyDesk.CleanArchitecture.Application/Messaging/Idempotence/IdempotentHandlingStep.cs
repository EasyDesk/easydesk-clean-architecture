using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Idempotence;

public class IdempotentHandlingStep : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        var messageId = context.Load<TransportMessage>().GetMessageId();
        var idempotenceManager = context
            .Load<ITransactionContext>()
            .GetServiceProvider()
            .GetRequiredService<IIdempotenceManager>();

        if (await idempotenceManager.HasBeenProcessed(messageId))
        {
            return;
        }

        await next();

        await idempotenceManager.MarkAsProcessed(messageId);
    }
}
