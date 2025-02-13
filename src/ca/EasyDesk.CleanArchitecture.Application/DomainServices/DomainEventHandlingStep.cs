using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public sealed class DomainEventHandlingStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    private readonly DomainEventScope _domainEventScope;

    public DomainEventHandlingStep(DomainEventScope domainEventScope)
    {
        _domainEventScope = domainEventScope;
    }

    public bool IsForEachHandler => true;

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return await _domainEventScope.RunInScope(next.Invoke);
    }
}
