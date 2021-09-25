namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories
{
    public static partial class Repository
    {
        public interface ISaveRepository<T>
        {
            void Save(T entity);
        }
    }
}
