namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

/// <summary>
/// Extend this class to declare a wrapper of a pure value.
/// In order to access the wrapped value the users of your class
/// will be able to use your defined properties and methods
/// as well as using directly this class as the wrapped value,
/// thanks to the implicit conversion.
/// </summary>
/// <typeparam name="T">The wrapped value type.</typeparam>
/// <typeparam name="S">The type extending this record.</typeparam>
public abstract record ValueWrapper<T, S> : AbstractValueWrapper<T, S>
    where T : notnull, IEquatable<T>
    where S : AbstractValueWrapper<T, S>
{
    protected ValueWrapper(T value) : base(value)
    {
    }

    public static implicit operator T(ValueWrapper<T, S> wrapper) => wrapper.Value;
}
