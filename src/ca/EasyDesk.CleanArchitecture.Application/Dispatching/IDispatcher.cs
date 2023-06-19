namespace EasyDesk.CleanArchitecture.Application.Dispatching;

public interface IDispatcher
{
    Task<Result<R>> Dispatch<X, R>(IDispatchable<X> dispatchable, AsyncFunc<X, R> mapper);

    public Task<Result<X>> Dispatch<X>(IDispatchable<X> dispatchable) =>
        Dispatch(dispatchable, x => Task.FromResult(x));
}
