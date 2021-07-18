namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories
{
    public static partial class Repository
    {
        public interface IRemoveRepository<T>
        {
            void Remove(T entity);
        }
    }
}
