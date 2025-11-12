namespace EasyDesk.Commons.Observables;

public interface IObservable<out T>
{
    ISubscription Subscribe(Action<T> action);
}

public static class ObservableExtensions
{
    public static ISubscription Subscribe<T>(this IObservable<T> observable, Action action) =>
        observable.Subscribe(_ => action());
}
