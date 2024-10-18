using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal class SagaEventHandler<T, TId, TState> : AbstractSagaHandler<T, Nothing, TId, TState>, IDomainEventHandler<T>
    where T : DomainEvent
{
    public SagaEventHandler(
        ISagaCoordinator<TId, TState> coordinator,
        IServiceProvider serviceProvider,
        SagaStepConfiguration<T, Nothing, TId, TState> configuration)
        : base(coordinator, serviceProvider, configuration)
    {
    }
}
