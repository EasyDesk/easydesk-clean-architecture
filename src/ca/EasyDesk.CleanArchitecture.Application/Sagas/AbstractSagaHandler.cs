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

    public async Task<Result<R>> Handle(T request)
    {
        var sagaId = _configuration.GetSagaId(request);
        var existingSaga = await _coordinator.FindSaga(sagaId);
        if (existingSaga.IsAbsent && !_configuration.CanInitialize)
        {
            return await _configuration.HandleMissingSaga(_serviceProvider, sagaId, request);
        }

        var saga = await existingSaga.MatchAsync(
            some: c => Task.FromResult(Success(c)),
            none: () => GetNewSagaIfPossible(sagaId, request));

        return await saga.FlatMapAsync(c => _configuration.HandleStep(_serviceProvider, request, c));
    }

    private async Task<Result<SagaContext<TId, TState>>> GetNewSagaIfPossible(TId sagaId, T request)
    {
        return await _configuration
            .InitializeSaga(_serviceProvider, sagaId, request)
            .ThenMap(s => _coordinator.CreateNew(sagaId, s));
    }
}
