namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

/// <summary>
/// Extend this class to declare a wrapper of a physical quantity.
/// In order to access the wrapped value the user of your class
/// will need to call one of your defined properties.
/// You should define properties for each unit of measuring
/// that compute their value based on the wrapped value.
/// </summary>
/// <typeparam name="T">The wrapped value type.</typeparam>
/// <typeparam name="TSelf">The type extending this record.</typeparam>
public abstract record Quantity<T, TSelf> : AbstractValue<T, TSelf>, IComparable<Quantity<T, TSelf>>
    where T : struct, IEquatable<T>, IComparable<T>
    where TSelf : Quantity<T, TSelf>, IValue<T>
{
    protected Quantity(T value) : base(value)
    {
    }

    protected Quantity(T value, bool validate) : base(value, validate)
    {
    }

    public int CompareTo(Quantity<T, TSelf>? other) => InnerValue.CompareTo(other?.InnerValue ?? throw new InvalidOperationException("Cannot compare a quantity to null."));

    public static bool operator >(Quantity<T, TSelf> a, Quantity<T, TSelf> b) => a.CompareTo(b) > 0;

    public static bool operator <(Quantity<T, TSelf> a, Quantity<T, TSelf> b) => a.CompareTo(b) < 0;

    public static bool operator >=(Quantity<T, TSelf> a, Quantity<T, TSelf> b) => a.CompareTo(b) >= 0;

    public static bool operator <=(Quantity<T, TSelf> a, Quantity<T, TSelf> b) => a.CompareTo(b) <= 0;
}
