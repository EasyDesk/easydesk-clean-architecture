using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal class SagaEventHandler<T, TId, TState> : AbstractSagaHandler<T, Nothing, TId, TState>, IDomainEventHandler<T>
    where T : DomainEvent
{
    public SagaEventHandler(ISagaManager sagaManager, IServiceProvider serviceProvider, SagaEventConfiguration<T, TId, TState> configuration) : base(sagaManager, serviceProvider, configuration)
    {
    }
}
