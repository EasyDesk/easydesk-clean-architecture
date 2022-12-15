using EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.Tools.Collections;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;

public abstract class EfCoreRepository<TAggregate, TPersistence, TContext> :
    ISaveRepository<TAggregate>,
    IRemoveRepository<TAggregate>,
    ISaveAndHydrateRepository<TAggregate>
    where TContext : DbContext
    where TPersistence : class, IPersistenceModel<TAggregate, TPersistence>
    where TAggregate : AggregateRoot
{
    private readonly TContext _context;
    private readonly IDomainEventNotifier _eventNotifier;
    private readonly AggregatesTracker<TAggregate, TPersistence> _tracker = new();

    public EfCoreRepository(TContext context, IDomainEventNotifier eventNotifier)
    {
        _context = context;
        _eventNotifier = eventNotifier;
    }

    protected virtual DbSet<TPersistence> GetDbSet(TContext context) => context.Set<TPersistence>();

    protected DbSet<TPersistence> DbSet => GetDbSet(_context);

    private IQueryable<TPersistence> InitialQuery() => Includes(DbSet);

    protected async Task<Option<TAggregate>> GetSingle(QueryWrapper<TPersistence> queryWrapper)
    {
        var persistenceModel = await queryWrapper(InitialQuery()).FirstOptionAsync();
        return persistenceModel.Map(_tracker.TrackFromPersistenceModel);
    }

    protected async Task<IEnumerable<TAggregate>> GetMany(QueryWrapper<TPersistence> queryWrapper)
    {
        var persistenceModels = await queryWrapper(InitialQuery()).ToListAsync();
        return persistenceModels.Select(_tracker.TrackFromPersistenceModel);
    }

    public void Save(TAggregate aggregate)
    {
        var (persistenceModel, wasTracked) = _tracker.TrackFromAggregate(aggregate);

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

        NotifyAllEvents(aggregate);
    }

    public async Task<TAggregate> SaveAndHydrate(TAggregate aggregate)
    {
        var (persistenceModel, wasTracked) = _tracker.TrackFromAggregate(aggregate);

        if (wasTracked)
        {
            DbSet.Update(persistenceModel);
        }
        else
        {
            await DbSet.AddAsync(persistenceModel);
        }

        var newAggregate = _tracker.ReHydrate(aggregate);

        if (!wasTracked)
        {
            newAggregate.NotifyCreation();
        }

        NotifyAllEvents(newAggregate);

        return newAggregate;
    }

    public void Remove(TAggregate aggregate)
    {
        var persistenceModel = _tracker.GetPersistenceModel(aggregate);
        DbSet.Remove(persistenceModel);
        aggregate.NotifyRemoval();
        NotifyAllEvents(aggregate);
    }

    private void NotifyAllEvents(TAggregate aggregate) =>
        aggregate.ConsumeAllEvents().ForEach(_eventNotifier.Notify);

    protected abstract IQueryable<TPersistence> Includes(IQueryable<TPersistence> initialQuery);
}
