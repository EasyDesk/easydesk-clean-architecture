using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public sealed class DomainEventHandlingStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    private readonly IDomainEventFlusher _domainEventFlusher;

    public DomainEventHandlingStep(IDomainEventFlusher domainEventFlusher)
    {
        _domainEventFlusher = domainEventFlusher;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return await next().ThenFlatTapAsync(_ => _domainEventFlusher.Flush());
    }
}
