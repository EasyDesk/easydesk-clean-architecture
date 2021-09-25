namespace EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion
{
    public interface IModelConverter<TDomain, TPersistence>
        where TPersistence : class, new()
    {
        public TDomain ToDomain(TPersistence model);

        public void ApplyChanges(TDomain origin, TPersistence destination);
    }

    public static class ModelConverterExtensions
    {
        public static TPersistence ToPersistence<TDomain, TPersistence>(this IModelConverter<TDomain, TPersistence> converter, TDomain aggregate)
            where TPersistence : class, new()
        {
            var persistenceModel = new TPersistence();
            converter.ApplyChanges(aggregate, persistenceModel);
            return persistenceModel;
        }
    }
}
