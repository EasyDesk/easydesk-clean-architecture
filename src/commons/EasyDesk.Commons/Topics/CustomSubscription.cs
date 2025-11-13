namespace EasyDesk.Commons.Topics;

public static class Subscription
{
    public static ISubscription FromUnsubscribeAction(Action unsubscribeAction) =>
        new CustomSubscription(unsubscribeAction);

    public static ISubscription Compose(params IEnumerable<ISubscription> subscriptions)
    {
        var subscriptionList = subscriptions.ToList();
        return FromUnsubscribeAction(() => subscriptionList.ForEach(s => s.Unsubscribe()));
    }

    public sealed class CustomSubscription : ISubscription
    {
        private readonly Action _unsubscribeAction;
        private bool _unsubscribed;

        public CustomSubscription(Action unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction;
        }

        public void Unsubscribe()
        {
            if (_unsubscribed)
            {
                throw new InvalidOperationException("Already unsubscribed");
            }
            _unsubscribed = true;
            _unsubscribeAction();
        }
    }
}
