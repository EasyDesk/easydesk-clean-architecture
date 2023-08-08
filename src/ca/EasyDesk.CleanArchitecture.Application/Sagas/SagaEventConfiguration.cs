using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public sealed class SagaEventConfiguration<T, TId, TState>
    : SagaStepConfiguration<T, Nothing, TId, TState>
    where T : DomainEvent
{
    public SagaEventConfiguration(
        Func<T, TId> sagaIdProperty,
        AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<Nothing>> handler,
        AsyncFunc<IServiceProvider, TId, T, Result<TState>> sagaInitializer)
        : base(sagaIdProperty, handler, sagaInitializer)
    {
    }
}
