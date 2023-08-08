using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal class SagaRequestHandler<T, R, TId, TState> : AbstractSagaHandler<T, R, TId, TState>, IHandler<T, R>
    where T : IDispatchable<R>
{
    public SagaRequestHandler(ISagaManager sagaManager, IServiceProvider serviceProvider, SagaRequestConfiguration<T, R, TId, TState> configuration) : base(sagaManager, serviceProvider, configuration)
    {
    }
}
