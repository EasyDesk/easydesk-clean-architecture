namespace EasyDesk.Commons.Topics;

public interface ITopic<out T>
{
    ISubscription Subscribe(Action<T> handler);
}
