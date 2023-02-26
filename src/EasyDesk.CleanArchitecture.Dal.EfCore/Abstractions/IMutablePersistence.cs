namespace EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;

public interface IMutablePersistence<TDomain, TPersistence> :
    IDomainPersistence<TDomain, TPersistence>
    where TPersistence : IMutablePersistence<TDomain, TPersistence>
{
    static abstract void ApplyChanges(TDomain origin, TPersistence destination);
}

public interface IEntityPersistenceWithHydration<TDomain, TPersistence, THydrationData> :
    IMutablePersistence<TDomain, TPersistence>
    where TPersistence : IEntityPersistenceWithHydration<TDomain, TPersistence, THydrationData>
{
    THydrationData GetHydrationData();
}
