namespace EasyDesk.Commons.Observables;

public sealed class SimpleSubscription : ISubscription
{
    private readonly Action _unsubscribeAction;
    private bool _unsubscribed;

    public SimpleSubscription(Action unsubscribeAction)
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
