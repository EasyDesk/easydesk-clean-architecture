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
        var existingSaga = await _coordinator.FindSaga(sagaId);
        if (existingSaga.IsAbsent)
        {
            var ignoreMissingSaga = IgnoreMissingSaga();
            if (ignoreMissingSaga)
            {
                return ignoreMissingSaga.Value;
            }
        }
        var context = existingSaga.OrElseError(Errors.NotFound) || await GetNewSagaIfPossible(sagaId, request);
        return await context.FlatMapAsync(c => _configuration.HandleStep(_serviceProvider, request, c));
    }

    private async Task<Result<SagaContext<TId, TState>>> GetNewSagaIfPossible(TId sagaId, T request)
    {
        return await _configuration
            .InitializeSaga(_serviceProvider, sagaId, request)
            .ThenMap(s => _coordinator.CreateNew(sagaId, s));
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
