using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Sagas.Builder;

public class SagaEventHandlerSelector<T, TId, TState> : SagaHandlerSelector<SagaEventHandlerSelector<T, TId, TState>, T, TId, TState>
    where T : DomainEvent
{
    internal SagaEventHandlerSelector(ISagaConfigurationSink<TId, TState> sink, Func<T, TId> correlationProperty)
        : base(sink, correlationProperty)
    {
    }

    public override void HandleWith(AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<Nothing>> handler)
    {
        var initializer = Initializer.OrElse((_, _, _) => Task.FromResult(Failure<TState>(Errors.Generic("Unable to start saga with event of type {eventType}", typeof(T).Name))));
        Sink.RegisterEventConfiguration<T>(new(CorrelationProperty, handler, initializer, IgnoringClosedSaga));
    }

    public void HandleWith(Func<IServiceProvider, ISagaStepEventHandler<T, TId, TState>> handlerFactory) =>
        HandleWith((p, r, c) => handlerFactory(p).Handle(r, c));

    public void HandleWith<H>() where H : ISagaStepEventHandler<T, TId, TState> =>
        HandleWith(p => p.GetRequiredService<H>());
}
