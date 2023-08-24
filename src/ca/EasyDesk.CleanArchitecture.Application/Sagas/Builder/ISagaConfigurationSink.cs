using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.Sagas.Builder;

internal interface ISagaConfigurationSink<TId, TState>
{
    void RegisterRequestConfiguration<T, R>(SagaStepConfiguration<T, R, TId, TState> configuration)
        where T : IDispatchable<R>;

    void RegisterRequestConfiguration<T>(SagaStepConfiguration<T, TId, TState> configuration)
        where T : IDispatchable<Nothing>;

    void RegisterEventConfiguration<T>(SagaStepConfiguration<T, TId, TState> configuration)
        where T : DomainEvent;
}
