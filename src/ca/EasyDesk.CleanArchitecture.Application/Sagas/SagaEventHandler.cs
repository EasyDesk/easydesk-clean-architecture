using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal class SagaEventHandler<T, TId, TState> : AbstractSagaHandler<T, TId, TState>, IDomainEventHandler<T>
    where T : DomainEvent
{
    public SagaEventHandler(ISagaManager sagaManager, IServiceProvider serviceProvider, SagaStepConfiguration<T, TId, TState> configuration)
        : base(sagaManager, serviceProvider, configuration)
    {
    }
}
