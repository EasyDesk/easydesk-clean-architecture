using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate
{
    public interface IPersonRepository :
        Repository.IGetByIdRepository<Person>,
        Repository.ISaveRepository<Person>,
        Repository.IRemoveRepository<Person>
    {
    }
}
