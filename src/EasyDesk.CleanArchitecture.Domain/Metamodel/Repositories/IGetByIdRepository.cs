using EasyDesk.Tools.Options;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories
{
    public static partial class Repository
    {
        public interface IGetByIdRepository<T>
        {
            Task<Option<T>> GetById(Guid id);
        }
    }
}
