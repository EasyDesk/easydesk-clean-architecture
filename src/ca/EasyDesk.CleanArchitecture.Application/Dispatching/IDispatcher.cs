namespace EasyDesk.CleanArchitecture.Application.Dispatching;

public interface IDispatcher
{
    Task<Result<R>> Dispatch<X, R>(IDispatchable<X> dispatchable, AsyncFunc<X, R> mapper) where R : notnull;
}

public static class DispatcherExtensions
{
    public static Task<Result<X>> Dispatch<X>(this IDispatcher dispatcher, IDispatchable<X> dispatchable) where X : notnull =>
        dispatcher.Dispatch(dispatchable, x => Task.FromResult(x));
}
