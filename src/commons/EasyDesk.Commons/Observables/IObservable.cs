namespace EasyDesk.Commons.Observables;

public interface IObservable<out T>
{
    ISubscription Subscribe(Action<T> handler);
}

public static class ObservableExtensions
{
    public static ISubscription Subscribe<T>(this IObservable<T> observable, Action handler) =>
        observable.Subscribe(_ => handler());
}
