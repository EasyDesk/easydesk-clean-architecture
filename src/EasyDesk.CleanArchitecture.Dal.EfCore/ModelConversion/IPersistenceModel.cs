namespace EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;

public interface IPersistenceModel<TDomain, TPersistence>
    where TPersistence : class, IPersistenceModel<TDomain, TPersistence>
{
    TDomain ToDomain();

    static abstract TPersistence CreateDefaultPersistenceModel();

    static abstract void ApplyChanges(TDomain origin, TPersistence destination);
}

public static class ModelConverterExtensions
{
    public static TPersistence ToPersistence<TDomain, TPersistence>(this TDomain aggregate)
        where TPersistence : class, IPersistenceModel<TDomain, TPersistence>
    {
        var persistenceModel = TPersistence.CreateDefaultPersistenceModel();
        TPersistence.ApplyChanges(aggregate, persistenceModel);
        return persistenceModel;
    }
}
