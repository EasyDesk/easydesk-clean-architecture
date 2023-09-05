namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

/// <summary>
/// Extend this class to declare a wrapper of a pure value.
/// In order to access the wrapped value the users of your class
/// will be able to use your defined properties and methods
/// as well as using directly this class as the wrapped value,
/// thanks to the implicit conversion.
/// </summary>
/// <typeparam name="T">The wrapped value type.</typeparam>
/// <typeparam name="TSelf">The type extending this record.</typeparam>
public abstract record PureValue<T, TSelf> : AbstractValue<T, TSelf>
    where T : notnull, IEquatable<T>
    where TSelf : PureValue<T, TSelf>, IValue<T>
{
    protected PureValue(T value) : base(value)
    {
    }

    protected PureValue(T value, bool process) : base(value, process)
    {
    }

    public T Value => InnerValue;

    public static implicit operator T(PureValue<T, TSelf> wrapper) => wrapper.Value;
}
