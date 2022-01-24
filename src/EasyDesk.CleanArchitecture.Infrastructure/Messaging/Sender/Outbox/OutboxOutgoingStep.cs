using EasyDesk.CleanArchitecture.Application.Data;
using Rebus.Pipeline;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Sender.Outbox;

public class OutboxOutgoingStep : IOutgoingStep
{
    private readonly ITransactionManager _transactionManager;
    private readonly IOutbox _outbox;
    private readonly IOutboxChannel _outboxChannel;

    public OutboxOutgoingStep(ITransactionManager transactionManager, IOutbox outbox, IOutboxChannel outboxChannel)
    {
        _transactionManager = transactionManager;
        _outbox = outbox;
        _outboxChannel = outboxChannel;
    }

    public Task Process(OutgoingStepContext context, Func<Task> next)
    {

    }
}
