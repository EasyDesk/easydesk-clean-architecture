using EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using EasyDesk.SampleApp.Infrastructure.EfCore.Model;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Repositories;

public class EfCorePetRepository : EfCoreRepositoryWithHydration<Pet, PetModel, int>, IPetRepository
{
    public EfCorePetRepository(SampleAppContext context, IDomainEventNotifier eventNotifier)
        : base(context.Pets, eventNotifier)
    {
    }

    public IAggregateView<Pet> FindById(int id) => Find(p => p.Id == id);

    public Task RemoveAll() => DbSet.ExecuteDeleteAsync();
}
