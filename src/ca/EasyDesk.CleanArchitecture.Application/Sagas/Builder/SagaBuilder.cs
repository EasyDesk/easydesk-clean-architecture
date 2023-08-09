using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.Sagas.Builder;

public sealed class SagaBuilder<TId, TState>
{
    private readonly ISagaConfigurationSink<TId, TState> _sink;

    internal SagaBuilder(ISagaConfigurationSink<TId, TState> sink)
    {
        _sink = sink;
    }

    public SagaCorrelationSelector<SagaRequestHandlerSelector<T, R, TId, TState>, T, TId> OnRequest<T, R>() where T : IDispatchable<R> =>
        new(correlation => new(_sink, correlation));

    public SagaCorrelationSelector<SagaRequestHandlerSelector<T, TId, TState>, T, TId> OnRequest<T>() where T : IDispatchable<Nothing> =>
        new(correlation => new(_sink, correlation));

    public SagaCorrelationSelector<SagaEventHandlerSelector<T, TId, TState>, T, TId> OnEvent<T>() where T : DomainEvent =>
        new(correlation => new(_sink, correlation));
}
