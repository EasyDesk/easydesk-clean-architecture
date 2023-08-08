using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public sealed class SagaBuilder<TId, TState>
{
    private readonly ISagaConfigurationSink<TId, TState> _sink;

    internal SagaBuilder(ISagaConfigurationSink<TId, TState> sink)
    {
        _sink = sink;
    }

    public SagaRequestCorrelationSelector<T, R, TId, TState> OnRequest<T, R>() where T : IDispatchable<R> =>
        new(_sink);

    public SagaRequestCorrelationSelector<T, Nothing, TId, TState> OnRequest<T>() where T : IDispatchable<Nothing> =>
        new(_sink);

    public SagaEventCorrelationSelector<T, TId, TState> OnEvent<T>() where T : DomainEvent =>
        new(_sink);
}

public class SagaRequestCorrelationSelector<T, R, TId, TState>
    where T : IDispatchable<R>
{
    internal SagaRequestCorrelationSelector(ISagaConfigurationSink<TId, TState> sink)
    {
        Sink = sink;
    }

    internal ISagaConfigurationSink<TId, TState> Sink { get; }

    public SagaRequestHandlerSelector<T, R, TId, TState> CorrelateWith(Func<T, TId> correlationProperty) => new(Sink, correlationProperty);
}

public class SagaRequestHandlerSelector<T, R, TId, TState>
    where T : IDispatchable<R>
{
    internal SagaRequestHandlerSelector(ISagaConfigurationSink<TId, TState> sink, Func<T, TId> correlationProperty)
    {
        Sink = sink;
        CorrelationProperty = correlationProperty;
    }

    public SagaRequestHandlerSelector<T, R, TId, TState> InitializeWith(AsyncFunc<IServiceProvider, TId, T, Result<TState>> initialState)
    {
        Initializer = initialState;
        return this;
    }

    internal AsyncFunc<IServiceProvider, TId, T, Result<TState>> Initializer { get; set; } = (_, _, _) => Task.FromResult(Failure<TState>(Errors.Generic("Unable to start saga with request of type {requestType}", typeof(T).Name)));

    internal ISagaConfigurationSink<TId, TState> Sink { get; }

    internal Func<T, TId> CorrelationProperty { get; }

    public SagaRequestHandlerSelector<T, R, TId, TState> InitializeWith(AsyncFunc<TId, T, Result<TState>> initialState) =>
        InitializeWith((_, i, r) => initialState(i, r));

    public SagaRequestHandlerSelector<T, R, TId, TState> InitializeWith(AsyncFunc<T, Result<TState>> initialState) =>
        InitializeWith((_, _, r) => initialState(r));

    public SagaRequestHandlerSelector<T, R, TId, TState> InitializeWith<H>(AsyncFunc<H, TId, T, Result<TState>> initialState) where H : notnull =>
        InitializeWith((p, i, r) => initialState(p.GetRequiredService<H>(), i, r));

    public SagaRequestHandlerSelector<T, R, TId, TState> InitializeWith<H>(AsyncFunc<H, T, Result<TState>> initialState) where H : notnull =>
        InitializeWith<H>((h, _, r) => initialState(h, r));

    public SagaRequestHandlerSelector<T, R, TId, TState> InitializeWith<H>(AsyncFunc<H, Result<TState>> initialState) where H : notnull =>
        InitializeWith<H>((h, _, _) => initialState(h));

    public SagaRequestHandlerSelector<T, R, TId, TState> InitializeWith(Func<IServiceProvider, TId, T, Result<TState>> initialState) =>
        InitializeWith((p, i, r) => Task.FromResult(initialState(p, i, r)));

    public SagaRequestHandlerSelector<T, R, TId, TState> InitializeWith(Func<TId, T, TState> initialState) =>
        InitializeWith((_, i, r) => initialState(i, r));

    public SagaRequestHandlerSelector<T, R, TId, TState> InitializeWith(Func<T, TState> initialState) =>
        InitializeWith((_, _, r) => initialState(r));

    public SagaRequestHandlerSelector<T, R, TId, TState> InitializeWith<H>(Func<H, TId, T, TState> initialState) where H : notnull =>
        InitializeWith((p, i, r) => initialState(p.GetRequiredService<H>(), i, r));

    public SagaRequestHandlerSelector<T, R, TId, TState> InitializeWith<H>(Func<H, T, TState> initialState) where H : notnull =>
        InitializeWith<H>((h, _, r) => initialState(h, r));

    public SagaRequestHandlerSelector<T, R, TId, TState> InitializeWith<H>(Func<H, TState> initialState) where H : notnull =>
        InitializeWith<H>((h, _, _) => initialState(h));

    public void HandleWith(AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<R>> handler)
    {
        Sink.RegisterConfiguration<T, R>(new(CorrelationProperty, handler, Initializer));
    }

    public void HandleWith(Func<IServiceProvider, ISagaStepRequestHandler<T, R, TId, TState>> handlerFactory) =>
        HandleWith((p, r, c) => handlerFactory(p).Handle(r, c));

    public void HandleWith<H>(AsyncFunc<H, T, SagaContext<TId, TState>, Result<R>> handler) where H : notnull =>
        HandleWith((p, r, c) => handler(p.GetRequiredService<H>(), r, c));

    public void HandleWith<H>() where H : ISagaStepRequestHandler<T, R, TId, TState> =>
        HandleWith(p => p.GetRequiredService<H>());
}

public class SagaEventCorrelationSelector<T, TId, TState>
    where T : DomainEvent
{
    internal SagaEventCorrelationSelector(ISagaConfigurationSink<TId, TState> sink)
    {
        Sink = sink;
    }

    internal ISagaConfigurationSink<TId, TState> Sink { get; }

    public SagaEventHandlerSelector<T, TId, TState> CorrelateWith(Func<T, TId> correlationProperty) => new(Sink, correlationProperty);
}

public class SagaEventHandlerSelector<T, TId, TState>
    where T : DomainEvent
{
    internal SagaEventHandlerSelector(ISagaConfigurationSink<TId, TState> sink, Func<T, TId> correlationProperty)
    {
        Sink = sink;
        CorrelationProperty = correlationProperty;
    }

    public SagaEventHandlerSelector<T, TId, TState> InitializeWith(AsyncFunc<IServiceProvider, TId, T, Result<TState>> initialState)
    {
        Initializer = initialState;
        return this;
    }

    internal AsyncFunc<IServiceProvider, TId, T, Result<TState>> Initializer { get; set; } = (_, _, _) => Task.FromResult(Failure<TState>(Errors.Generic("Unable to start saga with event of type {eventType}", typeof(T).Name)));

    internal ISagaConfigurationSink<TId, TState> Sink { get; }

    internal Func<T, TId> CorrelationProperty { get; }

    public SagaEventHandlerSelector<T, TId, TState> InitializeWith(AsyncFunc<TId, T, Result<TState>> initialState) =>
        InitializeWith((_, i, r) => initialState(i, r));

    public SagaEventHandlerSelector<T, TId, TState> InitializeWith(AsyncFunc<T, Result<TState>> initialState) =>
        InitializeWith((_, _, r) => initialState(r));

    public SagaEventHandlerSelector<T, TId, TState> InitializeWith<H>(AsyncFunc<H, TId, T, Result<TState>> initialState) where H : notnull =>
        InitializeWith((p, i, r) => initialState(p.GetRequiredService<H>(), i, r));

    public SagaEventHandlerSelector<T, TId, TState> InitializeWith<H>(AsyncFunc<H, T, Result<TState>> initialState) where H : notnull =>
        InitializeWith<H>((h, _, r) => initialState(h, r));

    public SagaEventHandlerSelector<T, TId, TState> InitializeWith<H>(AsyncFunc<H, Result<TState>> initialState) where H : notnull =>
        InitializeWith<H>((h, _, _) => initialState(h));

    public SagaEventHandlerSelector<T, TId, TState> InitializeWith(Func<IServiceProvider, TId, T, Result<TState>> initialState) =>
        InitializeWith((p, i, r) => Task.FromResult(initialState(p, i, r)));

    public SagaEventHandlerSelector<T, TId, TState> InitializeWith(Func<TId, T, TState> initialState) =>
        InitializeWith((_, i, r) => initialState(i, r));

    public SagaEventHandlerSelector<T, TId, TState> InitializeWith(Func<T, TState> initialState) =>
        InitializeWith((_, _, r) => initialState(r));

    public SagaEventHandlerSelector<T, TId, TState> InitializeWith<H>(Func<H, TId, T, TState> initialState) where H : notnull =>
        InitializeWith((p, i, r) => initialState(p.GetRequiredService<H>(), i, r));

    public SagaEventHandlerSelector<T, TId, TState> InitializeWith<H>(Func<H, T, TState> initialState) where H : notnull =>
        InitializeWith<H>((h, _, r) => initialState(h, r));

    public SagaEventHandlerSelector<T, TId, TState> InitializeWith<H>(Func<H, TState> initialState) where H : notnull =>
        InitializeWith<H>((h, _, _) => initialState(h));

    public void HandleWith(AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<Nothing>> handler)
    {
        Sink.RegisterConfiguration<T>(new(CorrelationProperty, handler, Initializer));
    }

    public void HandleWith(Func<IServiceProvider, ISagaStepEventHandler<T, TId, TState>> handlerFactory) =>
        HandleWith((p, r, c) => handlerFactory(p).Handle(r, c));

    public void HandleWith<H>(AsyncFunc<H, T, SagaContext<TId, TState>, Result<Nothing>> handler) where H : notnull =>
        HandleWith((p, r, c) => handler(p.GetRequiredService<H>(), r, c));

    public void HandleWith<H>() where H : ISagaStepEventHandler<T, TId, TState> =>
        HandleWith(p => p.GetRequiredService<H>());
}

internal interface ISagaConfigurationSink<TId, TState>
{
    void RegisterConfiguration<T, R>(SagaRequestConfiguration<T, R, TId, TState> configuration)
        where T : IDispatchable<R>;

    void RegisterConfiguration<T>(SagaEventConfiguration<T, TId, TState> configuration)
        where T : DomainEvent;
}
