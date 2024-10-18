using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal class SagaRequestHandler<T, R, TId, TState> : AbstractSagaHandler<T, R, TId, TState>, IHandler<T, R>
    where T : IDispatchable<R>
{
    public SagaRequestHandler(
        ISagaCoordinator<TId, TState> coordinator,
        IServiceProvider serviceProvider,
        SagaStepConfiguration<T, R, TId, TState> configuration)
        : base(coordinator, serviceProvider, configuration)
    {
    }
}
