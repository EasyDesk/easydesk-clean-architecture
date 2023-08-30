using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Sagas.Builder;

public abstract class SagaHandlerSelector<TSelf, T, R, TId, TState>
    where TSelf : SagaHandlerSelector<TSelf, T, R, TId, TState>
{
    internal SagaHandlerSelector(ISagaConfigurationSink<TId, TState> sink, Func<T, TId> correlationProperty)
    {
        Sink = sink;
        CorrelationProperty = correlationProperty;
    }

    internal ISagaConfigurationSink<TId, TState> Sink { get; }

    internal Func<T, TId> CorrelationProperty { get; }

    internal Option<AsyncFunc<IServiceProvider, TId, T, Result<TState>>> Initializer { get; private set; } = None;

    public virtual TSelf InitializeWith(AsyncFunc<IServiceProvider, TId, T, Result<TState>> initialState)
    {
        Initializer = Some(initialState);
        return (TSelf)this;
    }

    public TSelf InitializeWith(AsyncFunc<TId, T, Result<TState>> initialState) =>
        InitializeWith((_, i, r) => initialState(i, r));

    public TSelf InitializeWith(AsyncFunc<T, Result<TState>> initialState) =>
        InitializeWith((_, _, r) => initialState(r));

    public TSelf InitializeWith<H>(AsyncFunc<H, TId, T, Result<TState>> initialState) where H : notnull =>
        InitializeWith((p, i, r) => initialState(p.GetRequiredService<H>(), i, r));

    public TSelf InitializeWith<H>(AsyncFunc<H, T, Result<TState>> initialState) where H : notnull =>
        InitializeWith<H>((h, _, r) => initialState(h, r));

    public TSelf InitializeWith<H>(AsyncFunc<H, Result<TState>> initialState) where H : notnull =>
        InitializeWith<H>((h, _, _) => initialState(h));

    public TSelf InitializeWith(Func<IServiceProvider, TId, T, Result<TState>> initialState) =>
        InitializeWith((p, i, r) => Task.FromResult(initialState(p, i, r)));

    public TSelf InitializeWith(Func<TId, T, TState> initialState) =>
        InitializeWith((_, i, r) => initialState(i, r));

    public TSelf InitializeWith(Func<T, TState> initialState) =>
        InitializeWith((_, _, r) => initialState(r));

    public TSelf InitializeWith<H>(Func<H, TId, T, TState> initialState) where H : notnull =>
        InitializeWith((p, i, r) => initialState(p.GetRequiredService<H>(), i, r));

    public TSelf InitializeWith<H>(Func<H, T, TState> initialState) where H : notnull =>
        InitializeWith<H>((h, _, r) => initialState(h, r));

    public TSelf InitializeWith<H>(Func<H, TState> initialState) where H : notnull =>
        InitializeWith<H>((h, _, _) => initialState(h));

    public abstract void HandleWith(AsyncFunc<IServiceProvider, T, SagaContext<TId, TState>, Result<R>> handler);

    public void HandleWith<H>(AsyncFunc<H, T, SagaContext<TId, TState>, Result<R>> handler) where H : notnull =>
        HandleWith((p, r, c) => handler(p.GetRequiredService<H>(), r, c));
}

public abstract class SagaHandlerSelector<TSelf, T, TId, TState> : SagaHandlerSelector<TSelf, T, Nothing, TId, TState>
    where TSelf : SagaHandlerSelector<TSelf, T, TId, TState>
{
    internal SagaHandlerSelector(ISagaConfigurationSink<TId, TState> sink, Func<T, TId> correlationProperty) : base(sink, correlationProperty)
    {
    }

    internal bool IgnoringClosedSaga { get; private set; } = false;

    public TSelf IgnoreClosedSaga()
    {
        if (Initializer.IsPresent)
        {
            throw new InvalidOperationException("Having an initializer while ignoring closed sagas is currently not supported");
        }
        IgnoringClosedSaga = true;
        return (TSelf)this;
    }

    public override TSelf InitializeWith(AsyncFunc<IServiceProvider, TId, T, Result<TState>> initialState)
    {
        if (IgnoringClosedSaga)
        {
            throw new InvalidOperationException("Having an initializer while ignoring closed sagas is currently not supported");
        }
        return base.InitializeWith(initialState);
    }
}
