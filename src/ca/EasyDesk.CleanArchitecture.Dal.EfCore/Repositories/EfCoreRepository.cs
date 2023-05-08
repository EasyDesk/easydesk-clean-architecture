using EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;
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
    where TPersistence : class, IEntityPersistence<TAggregate, TPersistence>
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

    protected async Task<bool> Exists(QueryWrapper<TPersistence> queryWrapper) =>
        await queryWrapper(InitialQuery()).AnyAsync();

    protected async Task<bool> Exists(Expression<Func<TPersistence, bool>> predicate) =>
        await Exists(q => q.Where(predicate));

    protected async Task<Option<TAggregate>> GetSingle(QueryWrapper<TPersistence> queryWrapper)

    {
        var persistenceModel = await queryWrapper(InitialQuery()).FirstOptionAsync();
        return persistenceModel.Map(Tracker.TrackFromPersistenceModel);
    }

    protected async Task<Option<TAggregate>> GetSingle(Expression<Func<TPersistence, bool>> predicate) =>
        await GetSingle(q => q.Where(predicate));

    protected async Task<IEnumerable<TAggregate>> GetMany(QueryWrapper<TPersistence> queryWrapper)
    {
        var persistenceModels = await queryWrapper(InitialQuery()).ToListAsync();
        return persistenceModels.Select(Tracker.TrackFromPersistenceModel);
    }

    protected async Task<IEnumerable<TAggregate>> GetMany(Expression<Func<TPersistence, bool>> predicate) =>
        await GetMany(q => q.Where(predicate));

    public void Save(TAggregate aggregate)
    {
        var wasTracked = Tracker.IsTracked(aggregate);
        var wasSaved = Tracker.IsSaved(aggregate);
        var persistenceModel = Tracker.TrackFromAggregate(aggregate);

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
