using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public class SagaBuilder<TController, TId, TState>
    where TController : ISagaController<TController, TId, TState>
{
    private readonly ISagaConfigurationSink<TController, TId, TState> _sink;

    internal SagaBuilder(ISagaConfigurationSink<TController, TId, TState> sink)
    {
        _sink = sink;
    }

    public SagaCorrelationSelector<T, R, TController, TId, TState> On<T, R>() where T : IDispatchable<R> =>
        new(_sink);

    public SagaCorrelationSelector<T, Nothing, TController, TId, TState> On<T>() where T : IDispatchable<Nothing> =>
        On<T, Nothing>();
}

public class SagaCorrelationSelector<T, R, TController, TId, TState>
    where T : IDispatchable<R>
    where TController : ISagaController<TController, TId, TState>
{
    private readonly ISagaConfigurationSink<TController, TId, TState> _sink;

    internal SagaCorrelationSelector(ISagaConfigurationSink<TController, TId, TState> sink)
    {
        _sink = sink;
    }

    public SagaHandlerSelector<T, R, TController, TId, TState> CorrelateWith(Func<T, TId> correlationProperty) =>
        new(_sink, correlationProperty);
}

public class SagaHandlerSelector<T, R, TController, TId, TState>
    where T : IDispatchable<R>
    where TController : ISagaController<TController, TId, TState>
{
    private readonly ISagaConfigurationSink<TController, TId, TState> _sink;
    private readonly Func<T, TId> _correlationProperty;
    private AsyncFunc<TController, TId, T, Option<TState>> _initializer = (_, _, _) => Task.FromResult<Option<TState>>(None);

    internal SagaHandlerSelector(
        ISagaConfigurationSink<TController, TId, TState> sink,
        Func<T, TId> correlationProperty)
    {
        _sink = sink;
        _correlationProperty = correlationProperty;
    }

    public SagaHandlerSelector<T, R, TController, TId, TState> InitializeWith(Func<TController, TId, T, TState> initialState) =>
        InitializeWith((c, i, r) => Task.FromResult(initialState(c, i, r)));

    public SagaHandlerSelector<T, R, TController, TId, TState> InitializeWith(Func<TId, T, TState> initialState) =>
        InitializeWith((_, i, r) => initialState(i, r));

    public SagaHandlerSelector<T, R, TController, TId, TState> InitializeWith(Func<T, TState> initialState) =>
        InitializeWith((_, _, r) => initialState(r));

    public SagaHandlerSelector<T, R, TController, TId, TState> InitializeWith(AsyncFunc<TController, TId, T, TState> initialState)
    {
        _initializer = async (c, i, r) => Some(await initialState(c, i, r));
        return this;
    }

    public SagaHandlerSelector<T, R, TController, TId, TState> InitializeWith(AsyncFunc<TId, T, TState> initialState) =>
        InitializeWith((_, i, r) => initialState(i, r));

    public SagaHandlerSelector<T, R, TController, TId, TState> InitializeWith(AsyncFunc<T, TState> initialState) =>
        InitializeWith((_, _, r) => initialState(r));

    public void HandleWith(Func<TController, SagaHandlerDelegate<T, R, TId, TState>> handlerFactory)
    {
        _sink.RegisterConfiguration<T, R>(new(_correlationProperty, handlerFactory, _initializer));
    }
}

internal interface ISagaConfigurationSink<TController, TId, TState>
    where TController : ISagaController<TController, TId, TState>
{
    void RegisterConfiguration<T, R>(SagaRequestConfiguration<T, R, TController, TId, TState> configuration)
        where T : IDispatchable<R>;
}
