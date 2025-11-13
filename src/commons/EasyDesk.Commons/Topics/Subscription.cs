namespace EasyDesk.Commons.Topics;

public sealed class Subscription : ISubscription
{
    private readonly Action _unsubscribeAction;
    private bool _unsubscribed;

    public Subscription(Action unsubscribeAction)
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
