using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.Sagas.Builder;

internal interface ISagaConfigurationSink<TId, TState>
{
    void RegisterRequestConfiguration<T, R>(SagaStepConfiguration<T, R, TId, TState> configuration)
        where T : IDispatchable<R>;

    void RegisterEventConfiguration<T>(SagaStepConfiguration<T, Nothing, TId, TState> configuration)
        where T : DomainEvent;
}
