namespace EasyDesk.CleanArchitecture.Domain.Metamodel;

public abstract class AggregateRoot : Entity
{
    public void NotifyCreation() => OnCreation();

    public void NotifyRemoval() => OnRemoval();

    protected virtual void OnCreation()
    {
    }

    protected virtual void OnRemoval()
    {
    }
}
