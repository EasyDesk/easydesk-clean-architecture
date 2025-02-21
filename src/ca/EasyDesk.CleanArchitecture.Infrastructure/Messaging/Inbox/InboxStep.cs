using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;
using Rebus.Bus;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Inbox;

public sealed class InboxStep<T> : IPipelineStep<T, Nothing>
    where T : IReadWriteOperation, IIncomingMessage
{
    private readonly IInbox _inbox;

    public InboxStep(IInbox inbox)
    {
        _inbox = inbox;
    }

    public bool IsForEachHandler => false;

    public async Task<Result<Nothing>> Run(T request, NextPipelineStep<Nothing> next)
    {
        var messageId = MessageContext.Current.TransportMessage.GetMessageId();
        if (await _inbox.HasBeenProcessed(messageId))
        {
            return Ok;
        }

        var result = await next();

        return await result.IfSuccessAsync(_ => _inbox.MarkAsProcessed(messageId));
    }
}
