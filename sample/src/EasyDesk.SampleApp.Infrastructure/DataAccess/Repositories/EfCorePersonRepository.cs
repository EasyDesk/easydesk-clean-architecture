using EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;
using EasyDesk.SampleApp.Infrastructure.DataAccess.ModelConverters;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

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

    public Task<Result<Person>> GetById(Guid id) => GetSingle(q => q.Where(p => p.Id == id));
}
