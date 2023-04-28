using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal class SagaHandler<T, R, TId, TState> : IHandler<T, R>
    where T : IDispatchable<R>
{
    private readonly ISagaManager _sagaManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly SagaRequestConfiguration<T, R, TId, TState> _configuration;

    public SagaHandler(
        ISagaManager sagaManager,
        IServiceProvider serviceProvider,
        SagaRequestConfiguration<T, R, TId, TState> configuration)
    {
        _sagaManager = sagaManager;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public async Task<Result<R>> Handle(T request)
    {
        var sagaId = _configuration.GetSagaId(request);
        var existingSaga = await _sagaManager.Find<TId, TState>(sagaId);
        var saga = existingSaga.OrElseError(Errors.NotFound) || await GetNewSagaIfPossible(sagaId, request);
        return await saga.FlatMapAsync(s => HandleSaga(request, sagaId, s.State, s.Reference, existingSaga.IsAbsent));
    }

    private async Task<Result<R>> HandleSaga(T request, TId id, TState state, ISagaReference<TState> sagaReference, bool isNew)
    {
        var context = new SagaContext<TId, TState>(id, state, isNew);
        return await _configuration
            .HandleStep(_serviceProvider, request, context)
            .ThenIfSuccessAsync(_ => HandleSagaState(sagaReference, context));
    }

    private async Task<Result<(ISagaReference<TState> Reference, TState State)>> GetNewSagaIfPossible(TId sagaId, T request)
    {
        var state = await _configuration
            .InitializeSaga(_serviceProvider, sagaId, request);
        return state.Map(s => (_sagaManager.CreateNew<TId, TState>(sagaId), s));
    }

    private static async Task HandleSagaState(ISagaReference<TState> sagaReference, SagaContext<TId, TState> context)
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
