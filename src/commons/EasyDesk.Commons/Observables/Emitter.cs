namespace EasyDesk.Commons.Observables;

public class Emitter<T>
{
    private readonly ObservableImpl _observable = new();

    public IObservable<T> Observable => _observable;

    public void Emit(T value)
    {
        _observable.Emit(value);
    }

    private class ObservableImpl : IObservable<T>
    {
        private readonly List<Action<T>> _subscribers = [];

        public ISubscription Subscribe(Action<T> action)
        {
            _subscribers.Add(action);
            return new SimpleSubscription(() => _subscribers.Remove(action));
        }

        public void Emit(T value)
        {
            _subscribers.ForEach(s => s(value));
        }
    }
}
