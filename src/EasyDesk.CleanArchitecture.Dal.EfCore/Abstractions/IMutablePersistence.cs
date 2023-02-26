namespace EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;

public interface IMutablePersistence<TDomain, TPersistence>
{
    static abstract void ApplyChanges(TDomain origin, TPersistence destination);
}

public interface IMutablePersistenceWithHydration<TDomain, TPersistence, THydrationData> :
    IMutablePersistence<TDomain, TPersistence>
{
    THydrationData GetHydrationData();
}
