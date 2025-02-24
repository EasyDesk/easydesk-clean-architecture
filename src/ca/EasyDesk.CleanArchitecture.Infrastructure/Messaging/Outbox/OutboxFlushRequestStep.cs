using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

public class OutboxFlushRequestStep<T, R> : IPipelineStep<T, R>
{
    private readonly OutboxTransactionHelper _outboxTransactionHelper;

    public OutboxFlushRequestStep(OutboxTransactionHelper outboxTransactionHelper)
    {
        _outboxTransactionHelper = outboxTransactionHelper;
    }

    public bool IsForEachHandler => false;

    public Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return next().ThenIfSuccess(_ => _outboxTransactionHelper.RequestNewFlushIfNecessary());
    }
}
