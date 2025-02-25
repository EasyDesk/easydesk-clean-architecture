using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public sealed class DomainEventHandlingStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    private readonly DomainEventQueue _eventQueue;

    public DomainEventHandlingStep(DomainEventQueue eventQueue)
    {
        _eventQueue = eventQueue;
    }

    public bool IsForEachHandler => true;

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return await next().ThenFlatTapAsync(_ => _eventQueue.Flush());
    }
}
