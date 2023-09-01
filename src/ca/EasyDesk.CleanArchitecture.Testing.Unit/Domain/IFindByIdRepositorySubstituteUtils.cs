using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.Commons.Options;
using NSubstitute;
using NSubstitute.Core;

namespace EasyDesk.CleanArchitecture.Testing.Unit.Domain;

public static class IFindByIdRepositorySubstituteUtils
{
    public static ConfiguredCall Returns<T>(this IAggregateView<T> findByIdCall, Option<T> aggregateOption)
    {
        return findByIdCall.Returns(new AggregateViewProxy<T>(aggregateOption));
    }

    public static ConfiguredCall Returns<T>(this IAggregateView<T> findByIdCall, T aggregate)
    {
        return findByIdCall.Returns(Some(aggregate));
    }

    public static ConfiguredCall ReturnsForAnyArgs<T>(this IAggregateView<T> findByIdCall, Option<T> aggregateOption)
    {
        return findByIdCall.ReturnsForAnyArgs(new AggregateViewProxy<T>(aggregateOption));
    }

    public static ConfiguredCall ReturnsForAnyArgs<T>(this IAggregateView<T> findByIdCall, T aggregate)
    {
        return findByIdCall.ReturnsForAnyArgs(Some(aggregate));
    }
}
