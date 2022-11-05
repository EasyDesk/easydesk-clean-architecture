using EasyDesk.CleanArchitecture.Application.ContextProvider;
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
    private readonly IContextProvider _contextInfo;

    public InboxStep(IInbox inbox, IContextProvider contextInfo)
    {
        _inbox = inbox;
        _contextInfo = contextInfo;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        if (_contextInfo.Context is not AsyncMessageContext)
        {
            return await next();
        }

        var messageId = MessageContext.Current.TransportMessage.GetMessageId();
        if (await _inbox.HasBeenProcessed(messageId))
        {
            return default(R); // For now, this is the only available option since we cannot force message result to be nothing.
        }

        return await next().ThenIfSuccessAsync(_ => _inbox.MarkAsProcessed(messageId));
    }
}
