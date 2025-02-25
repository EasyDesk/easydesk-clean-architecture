using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

public class OutboxStoreMessagesStep<T, R> : IPipelineStep<T, R>
{
    private readonly IOutbox _outbox;

    public OutboxStoreMessagesStep(IOutbox outbox)
    {
        _outbox = outbox;
    }

    public bool IsForEachHandler => true;

    public Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return next().ThenIfSuccessAsync(_ => _outbox.StoreEnqueuedMessages());
    }
}
