using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public interface ISagaStepHandler<T, R, TId, TState>
{
    Task<Result<R>> Handle(T request, SagaContext<TId, TState> context);
}
