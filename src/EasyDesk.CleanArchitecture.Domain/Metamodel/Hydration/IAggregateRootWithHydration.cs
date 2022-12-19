namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Hydration;

public interface IAggregateRootWithHydration<T>
{
    void Hydrate(T data);
}
