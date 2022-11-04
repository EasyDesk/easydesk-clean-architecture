using EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools.Collections;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;

internal class AggregatesTracker<TAggregate, TPersistence>
    where TAggregate : AggregateRoot
    where TPersistence : IPersistenceModel<TAggregate, TPersistence>
{
    private readonly Dictionary<TAggregate, TPersistence> _modelsMap = new();

    public bool IsTracked(TAggregate aggregate) => _modelsMap.ContainsKey(aggregate);

    public TPersistence TrackFromAggregate(TAggregate aggregate)
    {
        if (IsTracked(aggregate))
        {
            var persistenceModel = _modelsMap[aggregate];
            TPersistence.ApplyChanges(aggregate, persistenceModel);
            return persistenceModel;
        }
        else
        {
            var persistenceModel = aggregate.ToPersistence<TAggregate, TPersistence>();
            _modelsMap.Add(aggregate, persistenceModel);
            return persistenceModel;
        }
    }

    public TAggregate TrackFromPersistenceModel(TPersistence persistenceModel)
    {
        var aggregate = persistenceModel.ToDomain();
        _modelsMap.Add(aggregate, persistenceModel);
        return aggregate;
    }

    public TAggregate ReHydrate(TAggregate aggregate)
    {
        var persistenceModel = _modelsMap[aggregate];
        var newAggregate = persistenceModel.ToDomain();
        _modelsMap.Remove(aggregate);
        _modelsMap.Add(newAggregate, persistenceModel);
        return newAggregate;
    }

    public TPersistence GetPersistenceModel(TAggregate aggregate)
    {
        return _modelsMap
            .GetOption(aggregate)
            .OrElseThrow(() => new InvalidOperationException("This aggregate is not being tracked"));
    }
}
