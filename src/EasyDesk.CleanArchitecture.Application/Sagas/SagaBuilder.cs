using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaBuilder<TController, TId, TState>
    where TController : ISagaController<TController, TId, TState>
{
    public ISagaBuilder<TController, TId, TState> Handle<T>(Func<T, TId> property, Func<TController, SagaHandlerDelegate<T, Nothing, TState>> handler)
        where T : IDispatchable<Nothing> =>
        Handle<T, Nothing>(property, handler);

    public ISagaBuilder<TController, TId, TState> Handle<T, R>(Func<T, TId> property, Func<TController, SagaHandlerDelegate<T, R, TState>> handler)
        where T : IDispatchable<R> =>
        For<T, R>(new(property, handler, canStartSaga: false));

    public ISagaBuilder<TController, TId, TState> StartWith<T>(Func<T, TId> property, Func<TController, SagaHandlerDelegate<T, Nothing, TState>> handler)
        where T : IDispatchable<Nothing> =>
        StartWith<T, Nothing>(property, handler);

    public ISagaBuilder<TController, TId, TState> StartWith<T, R>(Func<T, TId> property, Func<TController, SagaHandlerDelegate<T, R, TState>> handler)
        where T : IDispatchable<R> =>
        For<T, R>(new(property, handler, canStartSaga: true));

    ISagaBuilder<TController, TId, TState> For<T, R>(SagaRequestConfiguration<T, R, TController, TId, TState> configuration)
        where T : IDispatchable<R>;
}
