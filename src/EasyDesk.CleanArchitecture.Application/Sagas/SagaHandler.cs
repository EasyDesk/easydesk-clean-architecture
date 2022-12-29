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
        var existingSaga = await _sagaManager.Find<TId, TState>(sagaId);
        var saga = existingSaga || GetNewSagaIfPossible(sagaId);
        return await saga.MatchAsync(
            some: s => HandleSaga(request, s.Reference, s.State),
            none: () => Task.FromResult(Failure<R>(Errors.Generic("Unable to start saga with request of type {requestType}", typeof(T).Name))));
    }

    private async Task<Result<R>> HandleSaga(T request, ISagaReference<TState> sagaReference, TState state)
    {
        var context = new SagaContext<TState>(state);
        return await _configuration.GetHandler(_controller)(request, context)
            .ThenIfSuccessAsync(_ => HandleSagaState(sagaReference, context));
    }

    private Option<(ISagaReference<TState> Reference, TState State)> GetNewSagaIfPossible(TId sagaId) =>
        _configuration.CanStartSaga
            ? Some((_sagaManager.CreateNew<TId, TState>(sagaId), _controller.GetInitialState()))
            : None;

    private static async Task HandleSagaState(ISagaReference<TState> sagaReference, SagaContext<TState> context)
    {
        if (context.IsComplete)
        {
            await sagaReference.Delete();
        }
        else
        {
            await sagaReference.Save(context.State);
        }
    }
}
