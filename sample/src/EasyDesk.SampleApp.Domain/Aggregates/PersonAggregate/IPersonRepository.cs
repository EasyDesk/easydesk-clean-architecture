using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using System;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate
{
    public interface IPersonRepository :
        IGetByIdRepository<Person, Guid>,
        ISaveRepository<Person>,
        IRemoveRepository<Person>
    {
    }
}
