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
    where TPersistence : class, new()
    where TAggregate : AggregateRoot
{
    private readonly TContext _context;
    private readonly IDomainEventNotifier _eventNotifier;
    private readonly AggregatesTracker<TAggregate, TPersistence> _tracker;

    public EfCoreRepository(
        TContext context,
        IModelConverter<TAggregate, TPersistence> converter,
        IDomainEventNotifier eventNotifier)
    {
        _context = context;
        _tracker = new(converter);
        _eventNotifier = eventNotifier;
    }

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

    public async Task Save(TAggregate aggregate) => await SaveImpl(aggregate);

    public async Task<TAggregate> SaveAndHydrate(TAggregate aggregate) => await SaveImpl(aggregate, hydrateAfterSave: true);

    private async Task<TAggregate> SaveImpl(TAggregate aggregate, bool hydrateAfterSave = false)
    {
        var wasTracked = _tracker.IsTracked(aggregate);
        var persistenceModel = _tracker.TrackFromAggregate(aggregate);

        if (wasTracked)
        {
            DbSet.Update(persistenceModel);
        }
        else
        {
            DbSet.Add(persistenceModel);
        }

        await _context.SaveChangesAsync();

        var newAggregate = hydrateAfterSave ? _tracker.ReHydrate(aggregate) : aggregate;

        if (!wasTracked)
        {
            aggregate.NotifyCreation();
        }
        NotifyAllEvents(newAggregate);

        return newAggregate;
    }

    public async Task Remove(TAggregate aggregate)
    {
        var persistenceModel = _tracker.GetPersistenceModel(aggregate);
        DbSet.Remove(persistenceModel);
        await _context.SaveChangesAsync();
        aggregate.NotifyRemoval();
        NotifyAllEvents(aggregate);
    }

    private void NotifyAllEvents(TAggregate aggregate) =>
        aggregate.ConsumeAllEvents().ForEach(_eventNotifier.Notify);

    protected abstract DbSet<TPersistence> GetDbSet(TContext context);

    protected abstract IQueryable<TPersistence> Includes(IQueryable<TPersistence> initialQuery);
}
