using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Sagas.Builder;

public class SagaRequestHandlerSelector<T, R, TId, TState> : SagaHandlerSelector<SagaRequestHandlerSelector<T, R, TId, TState>, T, R, TId, TState>
    where T : IDispatchable<R>
{
    internal SagaRequestHandlerSelector(ISagaConfigurationSink<TId, TState> sink, Func<T, TId> correlationProperty)
        : base(sink, correlationProperty)
    {
    }

    public override void HandleWith(AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<R>> handler)
    {
        var initializer = Initializer.OrElse((_, _, _) => Task.FromResult(Failure<TState>(Errors.Generic("Unable to start saga with request of type {requestType}", typeof(T).Name))));
        Sink.RegisterRequestConfiguration<T, R>(new(CorrelationProperty, handler, initializer));
    }

    public void HandleWith(Func<IServiceProvider, ISagaStepRequestHandler<T, R, TId, TState>> handlerFactory) =>
        HandleWith((p, r, c) => handlerFactory(p).Handle(r, c));

    public void HandleWith<H>() where H : ISagaStepRequestHandler<T, R, TId, TState> =>
        HandleWith(p => p.GetRequiredService<H>());
}

public class SagaRequestHandlerSelector<T, TId, TState> : SagaHandlerSelector<SagaRequestHandlerSelector<T, TId, TState>, T, TId, TState>
    where T : IDispatchable<Nothing>
{
    internal SagaRequestHandlerSelector(ISagaConfigurationSink<TId, TState> sink, Func<T, TId> correlationProperty)
        : base(sink, correlationProperty)
    {
    }

    public override void HandleWith(AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<Nothing>> handler)
    {
        var initializer = Initializer.OrElse((_, _, _) => Task.FromResult(Failure<TState>(Errors.Generic("Unable to start saga with request of type {requestType}", typeof(T).Name))));
        Sink.RegisterRequestConfiguration<T>(new(CorrelationProperty, handler, initializer, IgnoringClosedSaga));
    }

    public void HandleWith(Func<IServiceProvider, ISagaStepRequestHandler<T, TId, TState>> handlerFactory) =>
        HandleWith((p, r, c) => handlerFactory(p).Handle(r, c));

    public void HandleWith<H>() where H : ISagaStepRequestHandler<T, TId, TState> =>
        HandleWith(p => p.GetRequiredService<H>());
}
