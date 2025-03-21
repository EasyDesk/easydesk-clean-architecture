using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Hydration;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;

public abstract class EfCoreRepositoryWithHydration<TAggregate, TPersistence, THydrationData> :
    EfCoreRepository<TAggregate, TPersistence>,
    ISaveAndHydrateRepository<TAggregate, THydrationData>
    where TPersistence : class, IAggregateRootModel<TAggregate, TPersistence>, IWithHydration<THydrationData>
    where TAggregate : AggregateRoot, IAggregateRootWithHydration<THydrationData>
{
    protected EfCoreRepositoryWithHydration(DbSet<TPersistence> dbSet, IDomainEventNotifier eventNotifier)
        : base(dbSet, eventNotifier)
    {
    }

    public async Task SaveAndHydrate(TAggregate aggregate)
    {
        var wasTracked = Tracker.IsTracked(aggregate);
        var wasSaved = Tracker.IsSaved(aggregate);
        var persistenceModel = Tracker.TrackFromAggregate(aggregate);

        DbSet.Entry(persistenceModel).IncrementVersion();

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
