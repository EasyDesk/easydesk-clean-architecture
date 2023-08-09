using EasyDesk.CleanArchitecture.Application.ErrorManagement;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal abstract class AbstractSagaHandler<T, R, TId, TState>
{
    private readonly ISagaManager _sagaManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly SagaStepConfiguration<T, R, TId, TState> _configuration;

    public AbstractSagaHandler(
        ISagaManager sagaManager,
        IServiceProvider serviceProvider,
        SagaStepConfiguration<T, R, TId, TState> configuration)
    {
        _sagaManager = sagaManager;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public virtual Option<Result<R>> IgnoreMissingSaga() => None;

    public async Task<Result<R>> Handle(T request)
    {
        var sagaId = _configuration.GetSagaId(request);
        var existingSaga = await _sagaManager.Find<TId, TState>(sagaId);
        if (existingSaga.IsAbsent)
        {
            var ignoreMissingSaga = IgnoreMissingSaga();
            if (ignoreMissingSaga)
            {
                return ignoreMissingSaga.Value;
            }
        }
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
        var state = await _configuration.InitializeSaga(_serviceProvider, sagaId, request);
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

internal abstract class AbstractSagaHandler<T, TId, TState> : AbstractSagaHandler<T, Nothing, TId, TState>
{
    private readonly SagaStepConfiguration<T, TId, TState> _configuration;

    protected AbstractSagaHandler(
        ISagaManager sagaManager,
        IServiceProvider serviceProvider,
        SagaStepConfiguration<T, TId, TState> configuration)
        : base(sagaManager, serviceProvider, configuration)
    {
        _configuration = configuration;
    }

    public override Option<Result<Nothing>> IgnoreMissingSaga() => _configuration.IgnoreClosedSaga ? Some(Ok) : base.IgnoreMissingSaga();
}
