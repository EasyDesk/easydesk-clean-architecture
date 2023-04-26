using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using Rebus.Bus;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Inbox;

public sealed class InboxStep<T> : IPipelineStep<T, Nothing>
    where T : IReadWriteOperation, IIncomingMessage
{
    private readonly IInbox _inbox;
    private readonly IContextProvider _contextProvider;

    public InboxStep(IInbox inbox, IContextProvider contextProvider)
    {
        _inbox = inbox;
        _contextProvider = contextProvider;
    }

    public async Task<Result<Nothing>> Run(T request, NextPipelineStep<Nothing> next)
    {
        if (_contextProvider.CurrentContext is not ContextInfo.AsyncMessage)
        {
            return await next();
        }

        var messageId = MessageContext.Current.TransportMessage.GetMessageId();
        if (await _inbox.HasBeenProcessed(messageId))
        {
            return Ok;
        }

        var result = await next();

        return await result.IfSuccessAsync(_ => _inbox.MarkAsProcessed(messageId));
    }
}
