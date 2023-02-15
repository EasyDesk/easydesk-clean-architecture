namespace EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;

public interface IPersistenceModel<TDomain, TPersistence>
    where TPersistence : IPersistenceModel<TDomain, TPersistence>
{
    TDomain ToDomain();

    static abstract TPersistence ToPersistence(TDomain origin);

    static abstract void ApplyChanges(TDomain origin, TPersistence destination);
}

public interface IPersistenceModelWithHydration<TDomain, TPersistence, THydrationData> :
    IPersistenceModel<TDomain, TPersistence>
    where TPersistence : IPersistenceModelWithHydration<TDomain, TPersistence, THydrationData>
{
    THydrationData GetHydrationData();
}
