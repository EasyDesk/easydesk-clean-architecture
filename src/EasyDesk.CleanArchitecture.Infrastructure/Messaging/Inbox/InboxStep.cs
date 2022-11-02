using EasyDesk.CleanArchitecture.Application.Cqrs.Operations;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.Messaging;
using Rebus.Bus;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Inbox;

public class InboxStep<T, R> : IPipelineStep<T, R>
    where T : IMessage, IReadWriteOperation
{
    private readonly IInbox _inbox;

    public InboxStep(IInbox inbox)
    {
        _inbox = inbox;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        // TODO: split logic between rebus and dispatching pipeline.
        var messageContext = MessageContext.Current;
        if (messageContext is null)
        {
            return await next();
        }

        var messageId = messageContext.TransportMessage.GetMessageId();
        if (await _inbox.HasBeenProcessed(messageId))
        {
            return default;
        }

        return await next().ThenIfSuccessAsync(_ => _inbox.MarkAsProcessed(messageId));
    }
}
