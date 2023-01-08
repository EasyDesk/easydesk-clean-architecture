using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

public interface IPersonRepository :
    IGetByIdRepository<Person, Guid>,
    ISaveRepository<Person>,
    IRemoveRepository<Person>
{
    Task RemoveAll();
}
