using System;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

/// <summary>
/// Extend this class to declare a wrapper of a physical quantity.
/// In order to access the wrapped value the user of your class
/// will need to call one of your defined properties.
/// You should define properties for each unit of measuring
/// that compute their value based on the wrapped value.
/// </summary>
/// <typeparam name="T">The wrapped value type.</typeparam>
/// <typeparam name="S">The type extending this record.</typeparam>
public abstract record QuantityWrapper<T, S> : AbstractValueWrapper<T, S>, IComparable<QuantityWrapper<T, S>>
    where T : IEquatable<T>, IComparable<T>
    where S : AbstractValueWrapper<T, S>
{
    protected QuantityWrapper(T value) : base(value)
    {
    }

    public int CompareTo(QuantityWrapper<T, S> other) => Value.CompareTo(other.Value);

    public static bool operator >(QuantityWrapper<T, S> a, QuantityWrapper<T, S> b) => a.CompareTo(b) > 0;

    public static bool operator <(QuantityWrapper<T, S> a, QuantityWrapper<T, S> b) => a.CompareTo(b) < 0;

    public static bool operator >=(QuantityWrapper<T, S> a, QuantityWrapper<T, S> b) => a.CompareTo(b) >= 0;

    public static bool operator <=(QuantityWrapper<T, S> a, QuantityWrapper<T, S> b) => a.CompareTo(b) <= 0;
}
