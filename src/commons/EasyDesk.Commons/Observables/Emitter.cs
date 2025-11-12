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
        private int _lastId = 1;
        private readonly List<(int Id, Action<T> Handler)> _subscribers = [];

        public ISubscription Subscribe(Action<T> handler)
        {
            var id = _lastId;
            _lastId++;

            _subscribers.Add((id, handler));
            return new SimpleSubscription(() => _subscribers.Remove((id, handler)));
        }

        public void Emit(T value)
        {
            _subscribers.ToList().ForEach(s => s.Handler(value));
        }
    }
}
