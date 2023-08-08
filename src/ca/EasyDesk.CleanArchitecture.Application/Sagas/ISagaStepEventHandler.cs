using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaStepEventHandler<T, TId, TState>
    where T : DomainEvent
{
    Task<Result<Nothing>> Handle(T request, SagaContext<TId, TState> context);
}
