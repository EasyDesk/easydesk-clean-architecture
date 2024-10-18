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

    public SagaCorrelationSelector<T, R, TId, TState> OnRequest<T, R>() where T : IDispatchable<R> =>
        new(c => _sink.RegisterRequestConfiguration(c));

    public SagaCorrelationSelector<T, Nothing, TId, TState> OnRequest<T>() where T : IDispatchable<Nothing> =>
        OnRequest<T, Nothing>();

    public SagaCorrelationSelector<T, Nothing, TId, TState> OnEvent<T>() where T : DomainEvent =>
        new(c => _sink.RegisterEventConfiguration(c));
}

public class SagaCorrelationSelector<T, R, TId, TState>
{
    private readonly Action<SagaStepConfiguration<T, R, TId, TState>> _registerHandler;

    internal SagaCorrelationSelector(Action<SagaStepConfiguration<T, R, TId, TState>> registerHandler)
    {
        _registerHandler = registerHandler;
    }

    public SagaHandlerSelector<T, R, TId, TState> CorrelateWith(Func<T, TId> correlationProperty) => new(_registerHandler, correlationProperty);
}
