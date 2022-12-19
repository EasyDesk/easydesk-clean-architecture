using EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Repositories;

public class EfCorePetRepository : EfCoreRepositoryWithHydration<Pet, PetModel, SampleAppContext, int>, IPetRepository
{
    public EfCorePetRepository(SampleAppContext context, IDomainEventNotifier eventNotifier)
        : base(context, eventNotifier)
    {
    }

    public Task<Option<Pet>> GetById(int id) => GetSingle(q => q.Where(p => p.Id == id));

    protected override IQueryable<PetModel> Includes(IQueryable<PetModel> initialQuery) => initialQuery;
}
