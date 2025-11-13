using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;

namespace EasyDesk.Commons.Observables;

public static class Observable
{
    public static ISubscription Subscribe<T>(this IObservable<T> observable, Action handler) =>
        observable.Subscribe(_ => handler());

    public static IObservable<R> Map<T, R>(this IObservable<T> observable, Func<T, R> mapper) =>
        Custom<R>(h => observable.Subscribe(t => h(mapper(t))));

    public static IObservable<T> Filter<T>(this IObservable<T> observable, Func<T, bool> predicate) =>
        Custom<T>(h => observable.Subscribe(t =>
        {
            if (predicate(t))
            {
                h(t);
            }
        }));

    public static IObservable<T> Compose<T>(params IFixedList<IObservable<T>> observables) =>
        Custom<T>(h =>
        {
            var subscriptions = observables.Select(o => o.Subscribe(h));
            return new Subscription(() => subscriptions.ForEach(s => s.Unsubscribe()));
        });

    private static IObservable<T> Custom<T>(Func<Action<T>, ISubscription> subscribe) =>
        new CustomObservable<T>(subscribe);

    private class CustomObservable<T> : IObservable<T>
    {
        private readonly Func<Action<T>, ISubscription> _subscribe;

        public CustomObservable(Func<Action<T>, ISubscription> subscribe)
        {
            _subscribe = subscribe;
        }

        public ISubscription Subscribe(Action<T> handler) => _subscribe(handler);
    }
}
