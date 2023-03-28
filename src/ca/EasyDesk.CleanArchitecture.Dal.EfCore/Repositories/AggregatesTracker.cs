using EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;

internal class AggregatesTracker<A, P>
    where A : AggregateRoot
    where P : IEntityPersistence<A, P>
{
    private readonly Dictionary<A, P> _modelsMap = new();

    public bool IsTracked(A aggregate) => _modelsMap.ContainsKey(aggregate);

    public (P Model, bool WasTracked) TrackFromAggregate(A aggregate)
    {
        if (IsTracked(aggregate))
        {
            var persistenceModel = _modelsMap[aggregate];
            P.ApplyChanges(aggregate, persistenceModel);
            return (persistenceModel, true);
        }
        else
        {
            var persistenceModel = P.ToPersistence(aggregate);
            _modelsMap.Add(aggregate, persistenceModel);
            return (persistenceModel, false);
        }
    }

    public A TrackFromPersistenceModel(P persistenceModel)
    {
        var aggregate = persistenceModel.ToDomain();
        _modelsMap.Add(aggregate, persistenceModel);
        return aggregate;
    }

    public P GetPersistenceModel(A aggregate)
    {
        return _modelsMap
            .GetOption(aggregate)
            .OrElseThrow(() => new InvalidOperationException("This aggregate is not being tracked"));
    }
}
