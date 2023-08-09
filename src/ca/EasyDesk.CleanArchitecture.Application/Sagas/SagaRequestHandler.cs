using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal class SagaRequestHandler<T, R, TId, TState> : AbstractSagaHandler<T, R, TId, TState>, IHandler<T, R>
    where T : IDispatchable<R>
{
    public SagaRequestHandler(ISagaManager sagaManager, IServiceProvider serviceProvider, SagaStepConfiguration<T, R, TId, TState> configuration)
        : base(sagaManager, serviceProvider, configuration)
    {
    }
}

internal class SagaRequestHandler<T, TId, TState> : AbstractSagaHandler<T, TId, TState>, IHandler<T>
    where T : IDispatchable<Nothing>
{
    public SagaRequestHandler(
        ISagaManager sagaManager,
        IServiceProvider serviceProvider,
        SagaStepConfiguration<T, TId, TState> configuration)
        : base(sagaManager, serviceProvider, configuration)
    {
    }
}
