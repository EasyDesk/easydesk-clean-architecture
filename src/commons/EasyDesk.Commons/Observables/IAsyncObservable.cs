using EasyDesk.Commons.Tasks;

namespace EasyDesk.Commons.Observables;

public interface IAsyncObservable<out T>
{
    ISubscription Subscribe(AsyncAction<T> handler);
}

public static class AsyncObservableExtensions
{
    public static ISubscription Subscribe<T>(this IAsyncObservable<T> observable, Action<T> handler)
    {
        return observable.Subscribe(t =>
        {
            handler(t);
            return Task.CompletedTask;
        });
    }

    public static ISubscription Subscribe<T>(this IAsyncObservable<T> observable, AsyncAction handler) =>
        observable.Subscribe(_ => handler());

    public static ISubscription Subscribe<T>(this IAsyncObservable<T> observable, Action handler) =>
        observable.Subscribe(_ => handler());
}
