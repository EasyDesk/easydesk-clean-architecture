using EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;
using EasyDesk.SampleApp.Infrastructure.DataAccess.ModelConverters;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Repositories;

public class EfCorePersonRepository : EfCoreRepository<Person, PersonModel, SampleAppContext>, IPersonRepository
{
    public EfCorePersonRepository(
        SampleAppContext context,
        IDomainEventNotifier eventNotifier) : base(context, new PersonConverter(), eventNotifier)
    {
    }

    protected override DbSet<PersonModel> GetDbSet(SampleAppContext context) => context.People;

    protected override IQueryable<PersonModel> Includes(IQueryable<PersonModel> initialQuery) => initialQuery;

    public Task<Option<Person>> GetById(Guid id) => GetSingle(q => q.Where(p => p.Id == id));
}
