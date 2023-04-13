using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public sealed class SagaBuilder<TId, TState>
    where TState : notnull
{
    private readonly ISagaConfigurationSink<TId, TState> _sink;

    internal SagaBuilder(ISagaConfigurationSink<TId, TState> sink)
    {
        _sink = sink;
    }

    public SagaCorrelationSelector<T, R, TId, TState> On<T, R>() where T : IDispatchable<R> where R : notnull =>
        new(_sink);

    public SagaCorrelationSelector<T, Nothing, TId, TState> On<T>() where T : IDispatchable<Nothing> =>
        On<T, Nothing>();
}

public sealed class SagaCorrelationSelector<T, R, TId, TState>
    where TState : notnull
    where R : notnull
    where T : IDispatchable<R>
{
    private readonly ISagaConfigurationSink<TId, TState> _sink;

    internal SagaCorrelationSelector(ISagaConfigurationSink<TId, TState> sink)
    {
        _sink = sink;
    }

    public SagaHandlerSelector<T, R, TId, TState> CorrelateWith(Func<T, TId> correlationProperty) =>
        new(_sink, correlationProperty);
}

public sealed class SagaHandlerSelector<T, R, TId, TState>
    where TState : notnull
    where R : notnull
    where T : IDispatchable<R>
{
    private readonly ISagaConfigurationSink<TId, TState> _sink;
    private readonly Func<T, TId> _correlationProperty;
    private AsyncFunc<IServiceProvider, TId, T, Result<TState>> _initializer = (_, _, _) => Task.FromResult(Failure<TState>(Errors.Generic("Unable to start saga with request of type {requestType}", typeof(T).Name)));

    internal SagaHandlerSelector(
        ISagaConfigurationSink<TId, TState> sink,
        Func<T, TId> correlationProperty)
    {
        _sink = sink;
        _correlationProperty = correlationProperty;
    }

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith(AsyncFunc<IServiceProvider, TId, T, Result<TState>> initialState)
    {
        _initializer = initialState;
        return this;
    }

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith(AsyncFunc<TId, T, Result<TState>> initialState) =>
        InitializeWith((_, i, r) => initialState(i, r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith(AsyncFunc<T, Result<TState>> initialState) =>
        InitializeWith((_, _, r) => initialState(r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith<H>(AsyncFunc<H, TId, T, Result<TState>> initialState) where H : notnull =>
        InitializeWith((p, i, r) => initialState(p.GetRequiredService<H>(), i, r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith<H>(AsyncFunc<H, T, Result<TState>> initialState) where H : notnull =>
        InitializeWith<H>((h, _, r) => initialState(h, r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith<H>(AsyncFunc<H, Result<TState>> initialState) where H : notnull =>
        InitializeWith<H>((h, _, _) => initialState(h));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith(Func<IServiceProvider, TId, T, Result<TState>> initialState) =>
        InitializeWith((p, i, r) => Task.FromResult(initialState(p, i, r)));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith(Func<TId, T, TState> initialState) =>
        InitializeWith((_, i, r) => initialState(i, r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith(Func<T, TState> initialState) =>
        InitializeWith((_, _, r) => initialState(r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith<H>(Func<H, TId, T, TState> initialState) where H : notnull =>
        InitializeWith((p, i, r) => initialState(p.GetRequiredService<H>(), i, r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith<H>(Func<H, T, TState> initialState) where H : notnull =>
        InitializeWith<H>((h, _, r) => initialState(h, r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith<H>(Func<H, TState> initialState) where H : notnull =>
        InitializeWith<H>((h, _, _) => initialState(h));

    public void HandleWith(AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<R>> handler)
    {
        _sink.RegisterConfiguration<T, R>(new(_correlationProperty, handler, _initializer));
    }

    public void HandleWith<H>(AsyncFunc<H, T, SagaContext<TId, TState>, Result<R>> handler) where H : notnull =>
        HandleWith((p, r, c) => handler(p.GetRequiredService<H>(), r, c));

    public void HandleWith(Func<IServiceProvider, ISagaStepHandler<T, R, TId, TState>> handlerFactory) =>
        HandleWith((p, r, c) => handlerFactory(p).Handle(r, c));

    public void HandleWith<H>() where H : ISagaStepHandler<T, R, TId, TState> =>
        HandleWith(p => p.GetRequiredService<H>());
}

internal interface ISagaConfigurationSink<TId, TState>
    where TState : notnull
{
    void RegisterConfiguration<T, R>(SagaRequestConfiguration<T, R, TId, TState> configuration)
        where R : notnull
        where T : IDispatchable<R>;
}
