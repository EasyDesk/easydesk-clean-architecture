using EasyDesk.CleanArchitecture.Application.DomainServices;

namespace EasyDesk.CleanArchitecture.Application.Cqrs.Pipeline;

public class DomainEventHandlingStep<TRequest, TResult> : IPipelineStep<TRequest, TResult>
    where TRequest : ICommand<TResult>
{
    private readonly DomainEventQueue _domainEventQueue;

    public DomainEventHandlingStep(DomainEventQueue domainEventQueue)
    {
        _domainEventQueue = domainEventQueue;
    }

    public async Task<Result<TResult>> Run(TRequest request, NextPipelineStep<TResult> next)
    {
        var response = await next();
        return await response.FlatTapAsync(_ => _domainEventQueue.Flush());
    }
}
