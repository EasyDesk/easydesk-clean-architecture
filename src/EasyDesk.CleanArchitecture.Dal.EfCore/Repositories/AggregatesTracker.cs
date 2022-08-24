using EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools.Collections;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;

public class AggregatesTracker<TAggregate, TPersistence>
    where TAggregate : AggregateRoot
    where TPersistence : class, new()
{
    private readonly Dictionary<TAggregate, TPersistence> _modelsMap = new();
    private readonly IModelConverter<TAggregate, TPersistence> _converter;

    public AggregatesTracker(IModelConverter<TAggregate, TPersistence> converter)
    {
        _converter = converter;
    }

    public bool IsTracked(TAggregate aggregate) => _modelsMap.ContainsKey(aggregate);

    public TPersistence TrackFromAggregate(TAggregate aggregate)
    {
        if (IsTracked(aggregate))
        {
            var persistenceModel = _modelsMap[aggregate];
            _converter.ApplyChanges(aggregate, persistenceModel);
            return persistenceModel;
        }
        else
        {
            var persistenceModel = _converter.ToPersistence(aggregate);
            _modelsMap.Add(aggregate, persistenceModel);
            return persistenceModel;
        }
    }

    public TAggregate TrackFromPersistenceModel(TPersistence persistenceModel)
    {
        var aggregate = _converter.ToDomain(persistenceModel);
        _modelsMap.Add(aggregate, persistenceModel);
        return aggregate;
    }

    public TAggregate ReHydrate(TAggregate aggregate)
    {
        var persistenceModel = _modelsMap[aggregate];
        var newAggregate = _converter.ToDomain(persistenceModel);
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
