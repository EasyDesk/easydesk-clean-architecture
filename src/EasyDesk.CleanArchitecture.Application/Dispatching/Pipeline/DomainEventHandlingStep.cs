using EasyDesk.CleanArchitecture.Application.Cqrs.Operations;
using EasyDesk.CleanArchitecture.Application.Cqrs.Queries;
using EasyDesk.CleanArchitecture.Application.DomainServices;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

public class DomainEventHandlingStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    private readonly IDomainEventFlusher _domainEventFlusher;

    public DomainEventHandlingStep(IDomainEventFlusher domainEventFlusher)
    {
        _domainEventFlusher = domainEventFlusher;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        if (request is IQuery)
        {
            return await next();
        }
        return await next().ThenFlatTapAsync(_ => _domainEventFlusher.Flush());
    }
}
