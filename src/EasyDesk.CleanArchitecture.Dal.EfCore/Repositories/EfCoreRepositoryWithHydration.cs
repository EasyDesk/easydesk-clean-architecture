using EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Hydration;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;

public abstract class EfCoreRepositoryWithHydration<TAggregate, TPersistence, TContext, THydrationData> :
    EfCoreRepository<TAggregate, TPersistence, TContext>,
    ISaveAndHydrateRepository<TAggregate, THydrationData>
    where TContext : DbContext
    where TPersistence : class, IEntityPersistenceWithHydration<TAggregate, TPersistence, THydrationData>
    where TAggregate : AggregateRoot, IAggregateRootWithHydration<THydrationData>
{
    protected EfCoreRepositoryWithHydration(TContext context, IDomainEventNotifier eventNotifier)
        : base(context, eventNotifier)
    {
    }

    public async Task SaveAndHydrate(TAggregate aggregate)
    {
        var (persistenceModel, wasTracked) = Tracker.TrackFromAggregate(aggregate);

        if (wasTracked)
        {
            DbSet.Update(persistenceModel);
        }
        else
        {
            await DbSet.AddAsync(persistenceModel);
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
