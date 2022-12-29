using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal class SagaHandler<T, R, TController, TId, TState> : IHandler<T, R>
    where T : IDispatchable<R>
    where TController : ISagaController<TController, TId, TState>
{
    private readonly ISagaManager _sagaManager;
    private readonly TController _controller;
    private readonly SagaRequestConfiguration<T, R, TController, TId, TState> _configuration;

    public SagaHandler(
        ISagaManager sagaManager,
        TController controller,
        SagaRequestConfiguration<T, R, TController, TId, TState> configuration)
    {
        _sagaManager = sagaManager;
        _controller = controller;
        _configuration = configuration;
    }

    public async Task<Result<R>> Handle(T request)
    {
        var sagaId = _configuration.GetSagaId(request);
        var reference = await GetSagaReference(sagaId);
        return await reference.MatchAsync(
            some: r => HandleSaga(request, r),
            none: () => Task.FromResult(Failure<R>(Errors.Generic("Unable to start saga with request of type {requestType}", typeof(T).Name))));
    }

    private async Task<Option<ISagaReference<TId, TState>>> GetSagaReference(TId sagaId)
    {
        var existingSagaReference = await _sagaManager.Find<TId, TState>(sagaId);
        return existingSagaReference || CreateNewSagaReferenceIfPossible(sagaId);
    }

    private Option<ISagaReference<TId, TState>> CreateNewSagaReferenceIfPossible(TId sagaId)
    {
        if (!_configuration.CanStartSaga)
        {
            return None;
        }
        return Some(_sagaManager.CreateNew(sagaId, _controller.GetInitialState()));
    }

    private async Task<Result<R>> HandleSaga(T request, ISagaReference<TId, TState> sagaReference)
    {
        var context = new SagaContext<TState>(sagaReference.State);
        return await _configuration.GetHandler(_controller)(request, context)
            .ThenIfSuccessAsync(_ => HandleSagaState(sagaReference, context));
    }

    private static async Task HandleSagaState(ISagaReference<TId, TState> sagaReference, SagaContext<TState> context)
    {
        if (context.IsComplete)
        {
            await sagaReference.Delete();
        }
        else
        {
            await sagaReference.SaveState();
        }
    }
}
