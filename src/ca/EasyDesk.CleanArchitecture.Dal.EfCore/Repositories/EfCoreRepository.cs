using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.Commons.Collections;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;

public abstract class EfCoreRepository<TAggregate, TPersistence, TContext> :
    ISaveRepository<TAggregate>,
    IRemoveRepository<TAggregate>
    where TContext : DbContext
    where TPersistence : class, IAggregateRootModel<TAggregate, TPersistence>
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

    private IQueryable<TPersistence> InitialQuery() => WrapInitialQuery(DbSet);

    protected IAggregateView<TAggregate> Find(Expression<Func<TPersistence, bool>> predicate) =>
        Find(q => q.Where(predicate));

    protected IAggregateView<TAggregate> Find(QueryWrapper<TPersistence> queryWrapper) =>
        Find(queryWrapper(InitialQuery()));

    protected IAggregateView<TAggregate> Find(IQueryable<TPersistence> query) =>
        new EfCoreAggregateView<TAggregate, TPersistence>(query, Tracker);

    protected async Task<IEnumerable<TAggregate>> GetMany(Expression<Func<TPersistence, bool>> predicate) =>
        await GetMany(q => q.Where(predicate));

    protected async Task<IEnumerable<TAggregate>> GetMany(QueryWrapper<TPersistence> queryWrapper) =>
        await GetMany(queryWrapper(InitialQuery()));

    protected async Task<IEnumerable<TAggregate>> GetMany(IQueryable<TPersistence> query)
    {
        var persistenceModels = await query.ToListAsync();
        return persistenceModels.Select(Tracker.TrackFromPersistenceModel);
    }

    public void Save(TAggregate aggregate)
    {
        var wasTracked = Tracker.IsTracked(aggregate);
        var wasSaved = Tracker.IsSaved(aggregate);
        var persistenceModel = Tracker.TrackFromAggregate(aggregate);

        Context.Entry(persistenceModel).IncrementVersion();

        if (!wasTracked)
        {
            DbSet.Add(persistenceModel);
        }
        else if (wasSaved)
        {
            DbSet.Update(persistenceModel);
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

    protected virtual IQueryable<TPersistence> WrapInitialQuery(IQueryable<TPersistence> initialQuery) => initialQuery;
}
