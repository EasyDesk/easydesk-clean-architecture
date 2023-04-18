using EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Infrastructure.EfCore;
using EasyDesk.SampleApp.Infrastructure.EfCore.Model;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Repositories;

public class EfCorePersonRepository : EfCoreRepository<Person, PersonModel, SampleAppContext>, IPersonRepository
{
    public EfCorePersonRepository(
        SampleAppContext context,
        IDomainEventNotifier eventNotifier) : base(context, eventNotifier)
    {
    }

    protected override IQueryable<PersonModel> Includes(IQueryable<PersonModel> initialQuery) => initialQuery;

    public Task<Option<Person>> GetById(Guid id) => GetSingle(q => q.Where(p => p.Id == id));

    public Task<bool> Exists(Guid id) => DbSet.AnyAsync(p => p.Id == id);

    public Task RemoveAll() => DbSet.ExecuteDeleteAsync();
}
