using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Repositories
{
    public abstract class EfCoreRepository<TDomain, TPersistence, TContext> :
        ISaveRepository<TDomain>,
        IRemoveRepository<TDomain>
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

        private TDomain ConvertAndStartTracking(TPersistence persistenceModel)
        {
            var aggregate = _converter.ToDomain(persistenceModel);
            _trackedAggregates.Add(aggregate, persistenceModel);
            return aggregate;
        }

        protected async Task<Option<TDomain>> GetSingle(QueryWrapper<TPersistence> queryWrapper)
        {
            var persistenceModel = await queryWrapper(InitialQuery()).FirstOptionAsync();
            return persistenceModel.Map(ConvertAndStartTracking);
        }

        public void Save(TDomain entity)
        {
            if (_trackedAggregates.ContainsKey(entity))
            {
                Update(entity);
            }
            else
            {
                Add(entity);
            }
        }

        private void Add(TDomain aggregate)
        {
            var persistenceModel = _converter.ToPersistence(aggregate);
            DbSet.Add(persistenceModel);
            NotifyAllEvents(aggregate);
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
            NotifyAllEvents(aggregate);
        }

        private TPersistence GetTrackedPersistenceModel(TDomain aggregate)
        {
            return _trackedAggregates
                .GetOption(aggregate)
                .OrElseThrow(() => new InvalidOperationException("Trying to update an aggregate that was not retrieved using this repository"));
        }

        private void NotifyAllEvents(TDomain aggregate) =>
            aggregate.ConsumeAllEvents().ForEach(ev => _eventNotifier.Notify(ev));

        protected abstract DbSet<TPersistence> GetDbSet(TContext context);

        protected abstract IQueryable<TPersistence> Includes(IQueryable<TPersistence> initialQuery);
    }
}
