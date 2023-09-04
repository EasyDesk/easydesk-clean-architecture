using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

internal abstract class AbstractSagaHandler<T, R, TId, TState>
{
    private readonly ISagaCoordinator<TId, TState> _coordinator;
    private readonly IServiceProvider _serviceProvider;
    private readonly SagaStepConfiguration<T, R, TId, TState> _configuration;

    public AbstractSagaHandler(
        ISagaCoordinator<TId, TState> coordinator,
        IServiceProvider serviceProvider,
        SagaStepConfiguration<T, R, TId, TState> configuration)
    {
        _coordinator = coordinator;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public virtual Option<Result<R>> IgnoreMissingSaga() => None;

    public async Task<Result<R>> Handle(T request)
    {
        var sagaId = _configuration.GetSagaId(request);
        var existingSagaState = await _coordinator.FindSaga(sagaId);
        if (existingSagaState.IsAbsent)
        {
            var ignoreMissingSaga = IgnoreMissingSaga();
            if (ignoreMissingSaga)
            {
                return ignoreMissingSaga.Value;
            }
        }
        var saga = existingSagaState.OrElseError(Errors.NotFound) || await GetNewSagaIfPossible(sagaId, request);
        return await saga.FlatMapAsync(s => HandleSaga(request, sagaId, s, existingSagaState.IsAbsent));
    }

    private async Task<Result<R>> HandleSaga(T request, TId id, TState state, bool isNew)
    {
        var context = new SagaContext<TId, TState>(id, state, isNew);
        return await _configuration
            .HandleStep(_serviceProvider, request, context)
            .ThenIfSuccess(_ => HandleSagaState(id, context));
    }

    private async Task<Result<TState>> GetNewSagaIfPossible(TId sagaId, T request)
    {
        return await _configuration
            .InitializeSaga(_serviceProvider, sagaId, request)
            .ThenIfSuccess(s => _coordinator.CreateNew(sagaId, s));
    }

    private void HandleSagaState(TId id, SagaContext<TId, TState> context)
    {
        if (context.IsComplete)
        {
            _coordinator.CompleteSaga(id);
        }
        else
        {
            _coordinator.SaveState(id, context.State);
        }
    }
}

internal abstract class AbstractSagaHandler<T, TId, TState> : AbstractSagaHandler<T, Nothing, TId, TState>
{
    private readonly SagaStepConfiguration<T, TId, TState> _configuration;

    protected AbstractSagaHandler(
        ISagaCoordinator<TId, TState> coordinator,
        IServiceProvider serviceProvider,
        SagaStepConfiguration<T, TId, TState> configuration)
        : base(coordinator, serviceProvider, configuration)
    {
        _configuration = configuration;
    }

    public override Option<Result<Nothing>> IgnoreMissingSaga() => _configuration.IgnoreClosedSaga ? Some(Ok) : base.IgnoreMissingSaga();
}
