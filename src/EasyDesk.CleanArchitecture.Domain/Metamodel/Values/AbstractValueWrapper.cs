using System;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

public abstract record AbstractValueWrapper<T>
    where T : IEquatable<T>
{
    protected T Value { get; }

    public AbstractValueWrapper(T value)
    {
        Validate(value);
        Value = value;
    }

    /// <summary>
    /// This method should check the passed value based on domain
    /// constraints and throw ArgumentException if it fails.
    /// </summary>
    /// <param name="value">The value passed in the constructor.</param>
    protected abstract void Validate(T value);
}
