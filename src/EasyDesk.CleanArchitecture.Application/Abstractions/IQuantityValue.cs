using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

namespace EasyDesk.CleanArchitecture.Application.Abstractions;

public interface IQuantityValue<TApplication, TDomain, TWrapped> : IObjectValue<TApplication, TDomain>
    where TApplication : IMappableFrom<TDomain, TApplication>
    where TDomain : QuantityWrapper<TWrapped, TDomain>
    where TWrapped : struct, IComparable<TWrapped>, IEquatable<TWrapped>
{
    TWrapped Value { get => ToDomainObject().Value; }

    string Unit { get; }
}
