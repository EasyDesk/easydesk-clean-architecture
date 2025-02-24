using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

public class OutboxStoreMessagesStep<T, R> : IPipelineStep<T, R>
{
    private readonly OutboxTransactionHelper _outboxTransactionHelper;

    public OutboxStoreMessagesStep(OutboxTransactionHelper outboxTransactionHelper)
    {
        _outboxTransactionHelper = outboxTransactionHelper;
    }

    public bool IsForEachHandler => true;

    public Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return next().ThenIfSuccess(_ => _outboxTransactionHelper.StoreEnqueuedMessagesIfNecessary());
    }
}
