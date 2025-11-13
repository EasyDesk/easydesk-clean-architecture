using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;

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

    public static ITopic<R> Scan<T, R>(this ITopic<T> topic, R seed, Func<R, T, R> combine)
    {
        var current = seed;
        return Custom<R>(h => topic.Subscribe(t =>
        {
            current = combine(current, t);
            h(current);
        }));
    }

    public static ITopic<T> FilterOption<T>(this ITopic<Option<T>> topic) => topic
        .Filter(x => x.IsPresent)
        .Map(x => x.Value);

    public static ITopic<R> FilterMap<T, R>(this ITopic<T> topic, Func<T, Option<R>> mapper) => topic
        .Map(mapper)
        .FilterOption();

    public static ITopic<T> Compose<T>(params IEnumerable<ITopic<T>> topics) =>
        Custom<T>(h =>
        {
            var subscriptions = topics.Select(o => o.Subscribe(h));
            return Subscription.Compose(subscriptions);
        });

    public static ITopic<(T Item, int Index)> ZipWithIndex<T>(this ITopic<T> topic, int start = 0, int increment = 1) =>
        topic.Scan((Item: default(T)!, Index: start - increment), (current, item) => (Item: item, Index: current.Index + increment));

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
