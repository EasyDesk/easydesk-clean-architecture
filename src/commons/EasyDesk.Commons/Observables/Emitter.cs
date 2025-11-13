namespace EasyDesk.Commons.Observables;

public class Emitter<T> : IObservable<T>
{
    private readonly List<Subscriber> _subscribers = [];

    public ISubscription Subscribe(Action<T> handler)
    {
        var subscriber = new Subscriber
        {
            Handler = handler,
        };

        _subscribers.Add(subscriber);
        return new Subscription(() => _subscribers.Remove(subscriber));
    }

    private class Subscriber
    {
        public required Action<T> Handler { get; init; }
    }

    public void Emit(T value)
    {
        _subscribers.ToList().ForEach(s => s.Handler(value));
    }
}
