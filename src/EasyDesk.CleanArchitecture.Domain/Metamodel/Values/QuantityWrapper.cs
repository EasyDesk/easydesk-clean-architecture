using System;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

/// <summary>
/// Extend this class to declare a wrapper of a physical quantity.
/// In order to access the wrapped value the user of your class
/// will need to call one of your defined properties.
/// You should define properties for each unit of measuring
/// that compute their value based on the wrapped value.
/// </summary>
/// <typeparam name="T">The same record that extends this one.</typeparam>
public abstract record QuantityWrapper<T>
    : AbstractValueWrapper<T>, IComparable<QuantityWrapper<T>>
    where T : IEquatable<T>, IComparable<T>
{
    public QuantityWrapper(T value) : base(value)
    {
    }

    public int CompareTo(QuantityWrapper<T> other) => Value.CompareTo(other.Value);

    public static bool operator >(QuantityWrapper<T> a, QuantityWrapper<T> b) => a.CompareTo(b) > 0;

    public static bool operator <(QuantityWrapper<T> a, QuantityWrapper<T> b) => a.CompareTo(b) < 0;

    public static bool operator >=(QuantityWrapper<T> a, QuantityWrapper<T> b) => a.CompareTo(b) >= 0;

    public static bool operator <=(QuantityWrapper<T> a, QuantityWrapper<T> b) => a.CompareTo(b) <= 0;
}
