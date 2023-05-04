using EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Hydration;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;

public abstract class EfCoreRepositoryWithHydration<TAggregate, TPersistence, TContext, THydrationData> :
    EfCoreRepository<TAggregate, TPersistence, TContext>,
    ISaveAndHydrateRepository<TAggregate, THydrationData>
    where TContext : DbContext
    where TPersistence : class, IEntityPersistence<TAggregate, TPersistence>, IWithHydration<THydrationData>
    where TAggregate : AggregateRoot, IAggregateRootWithHydration<THydrationData>
{
    protected EfCoreRepositoryWithHydration(TContext context, IDomainEventNotifier eventNotifier)
        : base(context, eventNotifier)
    {
    }

    public async Task SaveAndHydrate(TAggregate aggregate)
    {
        var wasTracked = Tracker.IsTracked(aggregate);
        var wasSaved = Tracker.IsSaved(aggregate);
        var persistenceModel = Tracker.TrackFromAggregate(aggregate);

        if (!wasTracked)
        {
            await DbSet.AddAsync(persistenceModel);
        }
        else if (wasSaved)
        {
            DbSet.Update(persistenceModel);
        }

        var hydrationData = persistenceModel.GetHydrationData();
        aggregate.Hydrate(hydrationData);

        if (!wasTracked)
        {
            aggregate.NotifyCreation();
        }

        NotifyEmittedEvents(aggregate);
    }
}
