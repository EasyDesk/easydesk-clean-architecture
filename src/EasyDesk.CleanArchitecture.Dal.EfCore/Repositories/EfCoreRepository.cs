using EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.Tools.Collections;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;

public abstract class EfCoreRepository<TAggregate, TPersistence, TContext> :
    ISaveRepository<TAggregate>,
    IRemoveRepository<TAggregate>
    where TContext : DbContext
    where TPersistence : class, IPersistenceModel<TAggregate, TPersistence>
    where TAggregate : AggregateRoot
{
    private readonly IDomainEventNotifier _eventNotifier;

    public EfCoreRepository(TContext context, IDomainEventNotifier eventNotifier)
    {
        Context = context;
        _eventNotifier = eventNotifier;
        DbSet = context.Set<TPersistence>();
    }

    protected TContext Context { get; }

    internal AggregatesTracker<TAggregate, TPersistence> Tracker { get; } = new();

    protected DbSet<TPersistence> DbSet { get; }

    private IQueryable<TPersistence> InitialQuery() => Includes(DbSet);

    protected async Task<Option<TAggregate>> GetSingle(QueryWrapper<TPersistence> queryWrapper)
    {
        var persistenceModel = await queryWrapper(InitialQuery()).FirstOptionAsync();
        return persistenceModel.Map(Tracker.TrackFromPersistenceModel);
    }

    protected async Task<IEnumerable<TAggregate>> GetMany(QueryWrapper<TPersistence> queryWrapper)
    {
        var persistenceModels = await queryWrapper(InitialQuery()).ToListAsync();
        return persistenceModels.Select(Tracker.TrackFromPersistenceModel);
    }

    public void Save(TAggregate aggregate)
    {
        var (persistenceModel, wasTracked) = Tracker.TrackFromAggregate(aggregate);

        if (wasTracked)
        {
            DbSet.Update(persistenceModel);
        }
        else
        {
            DbSet.Add(persistenceModel);
        }

        if (!wasTracked)
        {
            aggregate.NotifyCreation();
        }

        NotifyEmittedEvents(aggregate);
    }

    public void Remove(TAggregate aggregate)
    {
        var persistenceModel = Tracker.GetPersistenceModel(aggregate);
        DbSet.Remove(persistenceModel);
        aggregate.NotifyRemoval();
        NotifyEmittedEvents(aggregate);
    }

    internal void NotifyEmittedEvents(TAggregate aggregate) =>
        aggregate.ConsumeAllEvents().ForEach(_eventNotifier.Notify);

    protected abstract IQueryable<TPersistence> Includes(IQueryable<TPersistence> initialQuery);
}
