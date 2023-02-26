namespace EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;

public interface IEntityPersistence<TDomain, TPersistence>
    : IMutablePersistence<TDomain, TPersistence>,
    IDomainPersistence<TDomain, TPersistence>
    where TPersistence : IMutablePersistence<TDomain, TPersistence>,
    IDomainPersistence<TDomain, TPersistence>
{
}

public interface IEntityPersistenceWithHydration<TDomain, TPersistence, THydrationData>
    : IMutablePersistenceWithHydration<TDomain, TPersistence, THydrationData>,
    IEntityPersistence<TDomain, TPersistence>
    where TPersistence : IMutablePersistenceWithHydration<TDomain, TPersistence, THydrationData>,
    IDomainPersistence<TDomain, TPersistence>
{
}
