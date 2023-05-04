using EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;

internal class AggregatesTracker<A, P>
    where A : AggregateRoot
    where P : IEntityPersistence<A, P>
{
    private record AggregatePersistenceStatus(P Persistence, bool Saved);

    private readonly Dictionary<A, AggregatePersistenceStatus> _modelsMap = new();

    public bool IsTracked(A aggregate) => _modelsMap.ContainsKey(aggregate);

    public bool IsSaved(A aggregate)
    {
        return _modelsMap
            .GetOption(aggregate)
            .Map(a => a.Saved)
            .OrElse(false);
    }

    public P TrackFromAggregate(A aggregate)
    {
        if (IsTracked(aggregate))
        {
            var persistenceModel = _modelsMap[aggregate];
            P.ApplyChanges(aggregate, persistenceModel.Persistence);
            return persistenceModel.Persistence;
        }
        else
        {
            var persistenceModel = P.ToPersistence(aggregate);
            _modelsMap.Add(aggregate, new(persistenceModel, Saved: false));
            return persistenceModel;
        }
    }

    public A TrackFromPersistenceModel(P persistenceModel)
    {
        var aggregate = persistenceModel.ToDomain();
        _modelsMap.Add(aggregate, new(persistenceModel, Saved: true));
        return aggregate;
    }

    public P GetPersistenceModel(A aggregate)
    {
        return _modelsMap
            .GetOption(aggregate)
            .Map(a => a.Persistence)
            .OrElseThrow(AggregateNotTracked);
    }

    private Exception AggregateNotTracked() => new InvalidOperationException("This aggregate is not being tracked");
}
