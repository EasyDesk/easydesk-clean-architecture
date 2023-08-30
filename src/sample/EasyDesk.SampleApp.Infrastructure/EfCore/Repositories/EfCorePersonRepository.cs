using EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Infrastructure.EfCore;
using EasyDesk.SampleApp.Infrastructure.EfCore.Model;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Repositories;

public class EfCorePersonRepository : EfCoreRepository<Person, PersonModel, SampleAppContext>, IPersonRepository
{
    public EfCorePersonRepository(
        SampleAppContext context,
        IDomainEventNotifier eventNotifier) : base(context, eventNotifier)
    {
    }

    public IAggregateView<Person> FindById(Guid id) => Find(p => p.Id == id);

    public Task RemoveAll() => DbSet.ExecuteDeleteAsync();
}
