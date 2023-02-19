using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;

public interface IEntityPersistence<TDomain, TPersistence> :
    IDomainPersistence<TDomain, TPersistence>
    where TPersistence : IEntityPersistence<TDomain, TPersistence>
    where TDomain : Entity
{
    static abstract void ApplyChanges(TDomain origin, TPersistence destination);
}

public interface IEntityPersistenceWithHydration<TDomain, TPersistence, THydrationData> :
    IEntityPersistence<TDomain, TPersistence>
    where TPersistence : IEntityPersistenceWithHydration<TDomain, TPersistence, THydrationData>
    where TDomain : Entity
{
    THydrationData GetHydrationData();
}
