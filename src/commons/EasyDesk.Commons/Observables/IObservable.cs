namespace EasyDesk.Commons.Observables;

public interface IObservable<out T>
{
    ISubscription Subscribe(Action<T> handler);
}
