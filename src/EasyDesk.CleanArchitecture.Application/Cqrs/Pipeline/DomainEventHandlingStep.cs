using EasyDesk.CleanArchitecture.Application.DomainServices;

namespace EasyDesk.CleanArchitecture.Application.Cqrs.Pipeline;

public class DomainEventHandlingStep<TRequest, TResult> : IPipelineStep<TRequest, TResult>
    where TRequest : ICommand<TResult>
{
    private readonly IDomainEventFlusher _domainEventFlusher;

    public DomainEventHandlingStep(IDomainEventFlusher domainEventFlusher)
    {
        _domainEventFlusher = domainEventFlusher;
    }

    public async Task<Result<TResult>> Run(TRequest request, NextPipelineStep<TResult> next)
    {
        var response = await next();
        return await response.FlatTapAsync(_ => _domainEventFlusher.Flush());
    }
}
