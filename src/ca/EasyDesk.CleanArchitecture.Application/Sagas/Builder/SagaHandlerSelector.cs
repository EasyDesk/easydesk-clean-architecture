using Autofac;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Sagas.Builder;

public record InvalidSagaInitializerType(string TypeName) : ApplicationError
{
    public override string GetDetail() => $"Unable to start saga with event of type {TypeName}";

    public static InvalidSagaInitializerType FromType<T>() => new(typeof(T).Name);
}

public class SagaHandlerSelector<T, R, TId, TState>
{
    private readonly Action<SagaStepConfiguration<T, R, TId, TState>> _registerHandler;
    private readonly Func<T, TId> _correlationProperty;
    private Option<AsyncFunc<IComponentContext, TId, T, Result<TState>>> _initializer = None;
    private AsyncFunc<IComponentContext, TId, T, Result<R>> _missingSagaHandler = (_, _, _) => DefaultMissingSagaHandler();

    internal SagaHandlerSelector(Action<SagaStepConfiguration<T, R, TId, TState>> registerHandler, Func<T, TId> correlationProperty)
    {
        _registerHandler = registerHandler;
        _correlationProperty = correlationProperty;
    }

    private static Task<Result<R>> DefaultMissingSagaHandler() =>
        Task.FromResult(Failure<R>(InvalidSagaInitializerType.FromType<T>()));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith(AsyncFunc<IComponentContext, TId, T, Result<TState>> initialState)
    {
        _initializer = Some(initialState);
        return this;
    }

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith(AsyncFunc<TId, T, Result<TState>> initialState) =>
        InitializeWith((_, i, r) => initialState(i, r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith(AsyncFunc<T, Result<TState>> initialState) =>
        InitializeWith((_, _, r) => initialState(r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith<H>(AsyncFunc<H, TId, T, Result<TState>> initialState) where H : notnull =>
        InitializeWith((p, i, r) => initialState(p.Resolve<H>(), i, r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith<H>(AsyncFunc<H, T, Result<TState>> initialState) where H : notnull =>
        InitializeWith<H>((h, _, r) => initialState(h, r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith<H>(AsyncFunc<H, Result<TState>> initialState) where H : notnull =>
        InitializeWith<H>((h, _, _) => initialState(h));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith(Func<IComponentContext, TId, T, Result<TState>> initialState) =>
        InitializeWith((p, i, r) => Task.FromResult(initialState(p, i, r)));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith(Func<TId, T, TState> initialState) =>
        InitializeWith((_, i, r) => initialState(i, r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith(Func<T, TState> initialState) =>
        InitializeWith((_, _, r) => initialState(r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith<H>(Func<H, TId, T, TState> initialState) where H : notnull =>
        InitializeWith((p, i, r) => initialState(p.Resolve<H>(), i, r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith<H>(Func<H, T, TState> initialState) where H : notnull =>
        InitializeWith<H>((h, _, r) => initialState(h, r));

    public SagaHandlerSelector<T, R, TId, TState> InitializeWith<H>(Func<H, TState> initialState) where H : notnull =>
        InitializeWith<H>((h, _, _) => initialState(h));

    public SagaHandlerSelector<T, R, TId, TState> HandleMissingSaga(AsyncFunc<IComponentContext, TId, T, Result<R>> handler)
    {
        _missingSagaHandler = handler;
        return this;
    }

    public SagaHandlerSelector<T, R, TId, TState> HandleMissingSaga(AsyncFunc<TId, T, Result<R>> handler) =>
        HandleMissingSaga((_, i, r) => handler(i, r));

    public SagaHandlerSelector<T, R, TId, TState> HandleMissingSaga(AsyncFunc<T, Result<R>> handler) =>
        HandleMissingSaga((_, _, r) => handler(r));

    public SagaHandlerSelector<T, R, TId, TState> HandleMissingSaga(AsyncFunc<Result<R>> handler) =>
        HandleMissingSaga((_, _, _) => handler());

    public void HandleWith(AsyncFunc<IComponentContext, T, SagaContext<TId, TState>, Result<R>> handler)
    {
        _registerHandler(new(_correlationProperty, handler, _initializer, _missingSagaHandler));
    }

    public void HandleWith<H>(AsyncFunc<H, T, SagaContext<TId, TState>, Result<R>> handler) where H : notnull =>
        HandleWith((p, r, c) => handler(p.Resolve<H>(), r, c));

    public void HandleWith(Func<IComponentContext, ISagaStepHandler<T, R, TId, TState>> handlerFactory) =>
        HandleWith((p, r, c) => handlerFactory(p).Handle(r, c));

    public void HandleWith<H>() where H : ISagaStepHandler<T, R, TId, TState> =>
        HandleWith(p => p.Resolve<H>());
}

public static class SagaHandlerSelectorExtensions
{
    public static SagaHandlerSelector<T, Nothing, TId, TState> IgnoreMissingSaga<T, TId, TState>(this SagaHandlerSelector<T, Nothing, TId, TState> selector) =>
        selector.IgnoreMissingSaga(Nothing.Value);

    public static SagaHandlerSelector<T, R, TId, TState> IgnoreMissingSaga<T, R, TId, TState>(this SagaHandlerSelector<T, R, TId, TState> selector, R result) =>
        selector.HandleMissingSaga(() => Task.FromResult(Success(result)));
}
