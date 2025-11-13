using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;

namespace EasyDesk.Commons.Topics;

public static class Topic
{
    public static ISubscription Subscribe<T>(this ITopic<T> topic, Action handler) =>
        topic.Subscribe(_ => handler());

    public static ITopic<R> Map<T, R>(this ITopic<T> topic, Func<T, R> mapper) =>
        Custom<R>(h => topic.Subscribe(t => h(mapper(t))));

    public static ITopic<T> Filter<T>(this ITopic<T> topic, Func<T, bool> predicate) =>
        Custom<T>(h => topic.Subscribe(t =>
        {
            if (predicate(t))
            {
                h(t);
            }
        }));

    public static ITopic<T> Compose<T>(params IFixedList<ITopic<T>> topics) =>
        Custom<T>(h =>
        {
            var subscriptions = topics.Select(o => o.Subscribe(h));
            return new Subscription(() => subscriptions.ForEach(s => s.Unsubscribe()));
        });

    private static ITopic<T> Custom<T>(Func<Action<T>, ISubscription> subscribe) =>
        new CustomTopic<T>(subscribe);

    private class CustomTopic<T> : ITopic<T>
    {
        private readonly Func<Action<T>, ISubscription> _subscribe;

        public CustomTopic(Func<Action<T>, ISubscription> subscribe)
        {
            _subscribe = subscribe;
        }

        public ISubscription Subscribe(Action<T> handler) => _subscribe(handler);
    }
}
