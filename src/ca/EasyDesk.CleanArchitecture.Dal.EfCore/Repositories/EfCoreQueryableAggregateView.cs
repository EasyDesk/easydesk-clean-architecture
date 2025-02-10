using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.Commons.Options;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;

internal class EfCoreQueryableAggregateView<TAggregate, TPersistence> : IAggregateView<TAggregate>
    where TPersistence : class, IEntityPersistence<TAggregate, TPersistence>
    where TAggregate : AggregateRoot
{
    private readonly IQueryable<TPersistence> _query;
    private readonly AggregatesTracker<TAggregate, TPersistence> _tracker;

    public EfCoreQueryableAggregateView(IQueryable<TPersistence> query, AggregatesTracker<TAggregate, TPersistence> tracker)
    {
        _query = query;
        _tracker = tracker;
    }

    public Task<Option<TAggregate>> AsOption() => _query.FirstOptionAsync().ThenMap(_tracker.TrackFromPersistenceModel);

    public Task<bool> Exists() => _query.AnyAsync();
}
