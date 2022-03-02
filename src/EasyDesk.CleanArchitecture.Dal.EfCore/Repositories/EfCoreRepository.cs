using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.CleanArchitecture.Domain.Model.Errors;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.Results;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;

public abstract class EfCoreRepository<TDomain, TPersistence, TContext> :
    ISaveRepository<TDomain>,
    IRemoveRepository<TDomain>,
    ISaveAndHydrateRepository<TDomain>
    where TContext : DbContext
    where TPersistence : class, new()
    where TDomain : AggregateRoot
{
    private readonly TContext _context;
    private readonly IModelConverter<TDomain, TPersistence> _converter;
    private readonly IDomainEventNotifier _eventNotifier;
    private readonly Dictionary<TDomain, TPersistence> _trackedAggregates = new();

    public EfCoreRepository(
        TContext context,
        IModelConverter<TDomain, TPersistence> converter,
        IDomainEventNotifier eventNotifier)
    {
        _context = context;
        _converter = converter;
        _eventNotifier = eventNotifier;
    }

    protected DbSet<TPersistence> DbSet => GetDbSet(_context);

    private IQueryable<TPersistence> InitialQuery() => Includes(DbSet);

    private void StartTracking(TDomain aggregate, TPersistence persistenceModel) => _trackedAggregates.Add(aggregate, persistenceModel);

    private bool IsTracked(TDomain aggregate) => _trackedAggregates.ContainsKey(aggregate);

    private TPersistence GetTrackedPersistenceModel(TDomain aggregate)
    {
        return _trackedAggregates
            .GetOption(aggregate)
            .OrElseThrow(() => new InvalidOperationException("Trying to update an aggregate that was not retrieved using this repository"));
    }

    protected async Task<Result<TDomain>> GetSingle(QueryWrapper<TPersistence> queryWrapper)
    {
        var persistenceModel = await queryWrapper(InitialQuery()).FirstOptionAsync();
        return persistenceModel
            .Map(ConvertAndStartTracking)
            .OrElseError(AggregateNotFound.OfType<TDomain>);
    }

    protected async Task<IEnumerable<TDomain>> GetMany(QueryWrapper<TPersistence> queryWrapper)
    {
        var persistenceModels = await queryWrapper(InitialQuery()).ToListAsync();
        return persistenceModels.Select(ConvertAndStartTracking);
    }

    private TDomain ConvertAndStartTracking(TPersistence persistenceModel)
    {
        var aggregate = _converter.ToDomain(persistenceModel);
        StartTracking(aggregate, persistenceModel);
        return aggregate;
    }

    public void Save(TDomain aggregate)
    {
        if (IsTracked(aggregate))
        {
            Update(aggregate);
        }
        else
        {
            Add(aggregate);
        }
    }

    public async Task<TDomain> SaveAndHydrate(TDomain aggregate)
    {
        if (IsTracked(aggregate))
        {
            throw new InvalidOperationException("The given aggregate is already being tracked by this repository.");
        }
        var persistenceModel = _converter.ToPersistence(aggregate);
        await DbSet.AddAsync(persistenceModel);
        var hydratedAggregate = _converter.ToDomain(persistenceModel);
        MarkAggregateAsCreated(hydratedAggregate, persistenceModel);
        return hydratedAggregate;
    }

    private void Add(TDomain aggregate)
    {
        var persistenceModel = _converter.ToPersistence(aggregate);
        DbSet.Add(persistenceModel);
        MarkAggregateAsCreated(aggregate, persistenceModel);
    }

    private void Update(TDomain aggregate)
    {
        var persistenceModel = GetTrackedPersistenceModel(aggregate);
        _converter.ApplyChanges(aggregate, persistenceModel);
        DbSet.Update(persistenceModel);
        NotifyAllEvents(aggregate);
    }

    public void Remove(TDomain aggregate)
    {
        var persistenceModel = GetTrackedPersistenceModel(aggregate);
        DbSet.Remove(persistenceModel);
        MarkAggregateAsRemoved(aggregate);
    }

    private void MarkAggregateAsCreated(TDomain aggregate, TPersistence persistenceModel)
    {
        FlushAggregateEvents(aggregate);
        aggregate.NotifyCreation();
        NotifyAllEvents(aggregate);
        StartTracking(aggregate, persistenceModel);
    }

    private void MarkAggregateAsRemoved(TDomain aggregate)
    {
        aggregate.NotifyRemoval();
        NotifyAllEvents(aggregate);
    }

    private void NotifyAllEvents(TDomain aggregate) =>
        aggregate.ConsumeAllEvents().ForEach(_eventNotifier.Notify);

    private void FlushAggregateEvents(TDomain aggregate) =>
        aggregate.ConsumeAllEvents().ForEach(_ => { });

    protected abstract DbSet<TPersistence> GetDbSet(TContext context);

    protected abstract IQueryable<TPersistence> Includes(IQueryable<TPersistence> initialQuery);
}
