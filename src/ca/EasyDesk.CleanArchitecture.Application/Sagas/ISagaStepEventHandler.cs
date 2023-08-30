using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaStepEventHandler<T, TId, TState>
    where T : DomainEvent
{
    Task<Result<Nothing>> Handle(T request, SagaContext<TId, TState> context);
}
