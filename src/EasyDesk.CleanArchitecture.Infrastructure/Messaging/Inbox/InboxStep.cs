using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using Rebus.Bus;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Inbox;

public class InboxStep<T> : IPipelineStep<T, Nothing>
    where T : IReadWriteOperation, IIncomingMessage
{
    private readonly IInbox _inbox;
    private readonly IContextProvider _contextInfo;

    public InboxStep(IInbox inbox, IContextProvider contextInfo)
    {
        _inbox = inbox;
        _contextInfo = contextInfo;
    }

    public async Task<Result<Nothing>> Run(T request, NextPipelineStep<Nothing> next)
    {
        if (_contextInfo.Context is not AsyncMessageContext)
        {
            return await next();
        }

        var messageId = MessageContext.Current.TransportMessage.GetMessageId();
        if (await _inbox.HasBeenProcessed(messageId))
        {
            return Ok;
        }

        return await next().ThenIfSuccessAsync(_ => _inbox.MarkAsProcessed(messageId));
    }
}
