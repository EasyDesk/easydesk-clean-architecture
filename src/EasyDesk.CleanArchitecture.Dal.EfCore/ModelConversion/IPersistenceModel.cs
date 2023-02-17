namespace EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;

public interface IPersistenceObject<TDomain, TPersistence>
    where TPersistence : IPersistenceObject<TDomain, TPersistence>
{
    TDomain ToDomain();

    static abstract TPersistence ToPersistence(TDomain origin);
}

public interface IPersistenceModel<TDomain, TPersistence> :
    IPersistenceObject<TDomain, TPersistence>
    where TPersistence : IPersistenceModel<TDomain, TPersistence>
{
    static abstract void ApplyChanges(TDomain origin, TPersistence destination);
}

public interface IPersistenceModelWithHydration<TDomain, TPersistence, THydrationData> :
    IPersistenceModel<TDomain, TPersistence>
    where TPersistence : IPersistenceModelWithHydration<TDomain, TPersistence, THydrationData>
{
    THydrationData GetHydrationData();
}
